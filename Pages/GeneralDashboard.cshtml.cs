using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using CRM.Models.Entities;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http; // Added for session handling

namespace CRM.Pages
{
    [ValidateAntiForgeryToken]
    public class GeneralDashboardModel : PageModel
    {
        private readonly string _connectionString;
        private readonly IAntiforgery _antiforgery;

        public GeneralDashboardModel(IConfiguration configuration, IAntiforgery antiforgery)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
            _antiforgery = antiforgery;
            Console.WriteLine("GeneralDashboardModel constructor called");
        }

        public List<Case> Cases { get; set; } = new List<Case>();

        [BindProperty(SupportsGet = true)]
        public string? FilterName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterChannel { get; set; }

        [BindProperty]
        public Case NewCase { get; set; } = new Case();

        public string DebugMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            Console.WriteLine("DEBUG: OnGet called");
            DebugMessage = "Page loaded via OnGet";
            LoadCases();
            Console.WriteLine($"DEBUG: OnGet completed, Cases count: {Cases.Count}");
        }

        public IActionResult OnGetFilter()
        {
            Console.WriteLine($"DEBUG: OnGetFilter called with FilterName: {FilterName}, FilterChannel: {FilterChannel}");
            DebugMessage = $"Filtering with Name: {FilterName}, Channel: {FilterChannel}";
            LoadCases();
            Console.WriteLine($"DEBUG: OnGetFilter completed, Cases count: {Cases.Count}");
            return Page();
        }

        public IActionResult OnPostAdd()
        {
            Console.WriteLine("DEBUG: OnPostAdd handler invoked");
            DebugMessage = $"Received Form Data - EmpID: '{NewCase.EmpID}', CustomerName: '{NewCase.CustomerName}', " +
                           $"Contact: '{NewCase.Contact}', CaseChannel: '{NewCase.CaseChannel}', Description: '{NewCase.Description}'";

            Console.WriteLine("DEBUG: Checking ModelState");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("DEBUG: ModelState is invalid:");
                foreach (var entry in ModelState)
                {
                    Console.WriteLine($"DEBUG: Key: {entry.Key}");
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"DEBUG:   Error: {error.ErrorMessage}");
                        DebugMessage += $" | Validation Error: {error.ErrorMessage}";
                    }
                }
                DebugMessage += " | ModelState invalid";
                ViewData["Action"] = "Add";
                LoadCases();
                Console.WriteLine($"DEBUG: Returning Page due to invalid ModelState, Cases count: {Cases.Count}");
                return Page();
            }

            Console.WriteLine("DEBUG: Checking required fields");
            if (string.IsNullOrEmpty(NewCase.CustomerName) || string.IsNullOrEmpty(NewCase.Contact) ||
                string.IsNullOrEmpty(NewCase.CaseChannel) || string.IsNullOrEmpty(NewCase.Description))
            {
                Console.WriteLine("DEBUG: Form validation failed: One or more required fields are empty");
                DebugMessage += " | Form validation failed: One or more required fields are empty";
                ModelState.AddModelError("", "All fields are required.");
                ViewData["Action"] = "Add";
                LoadCases();
                Console.WriteLine($"DEBUG: Returning Page due to empty fields, Cases count: {Cases.Count}");
                return Page();
            }

            NewCase.CustomerID = Guid.NewGuid();
            NewCase.Status = "Open";
            NewCase.CreatedAt = DateTime.UtcNow;

            Console.WriteLine("DEBUG: Processing session EmpID");
            try
            {
                string sessionEmpId = HttpContext.Session.GetString("EmpID");
                if (string.IsNullOrEmpty(sessionEmpId))
                {
                    throw new InvalidOperationException("Session EmpID not set");
                }
                NewCase.EmpID = Guid.Parse(sessionEmpId);
                NewCase.CreatedBy = NewCase.EmpID;
                Console.WriteLine($"DEBUG: Parsed EmpID from session: {NewCase.EmpID}");
                DebugMessage += $" | Parsed EmpID from session: {NewCase.EmpID}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Session EmpID Error: {ex.Message}");
                DebugMessage += $" | Session EmpID Error: {ex.Message}";
                ModelState.AddModelError("", "Invalid session data. Please log in again.");
                ViewData["Action"] = "Add";
                LoadCases();
                Console.WriteLine($"DEBUG: Returning Page due to session error, Cases count: {Cases.Count}");
                return Page();
            }

            Console.WriteLine("DEBUG: Attempting database insert");
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    Console.WriteLine("DEBUG: Database connection opened");
                    DebugMessage += " | Database connection opened";

                    var checkUserCmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE EmpID = @EmpID", connection);
                    checkUserCmd.Parameters.AddWithValue("@EmpID", NewCase.EmpID);
                    int userExists = (int)checkUserCmd.ExecuteScalar();
                    Console.WriteLine($"DEBUG: User exists check for EmpID {NewCase.EmpID}: {userExists}");
                    DebugMessage += $" | User exists check for EmpID {NewCase.EmpID}: {userExists}";

                    if (userExists == 0)
                    {
                        Console.WriteLine($"DEBUG: EmpID {NewCase.EmpID} not found in Users table");
                        DebugMessage += $" | EmpID {NewCase.EmpID} not found in Users table";
                        ModelState.AddModelError("NewCase.EmpID", "Invalid Employee ID: Must match an existing user.");
                        ViewData["Action"] = "Add";
                        LoadCases();
                        Console.WriteLine($"DEBUG: Returning Page due to invalid EmpID, Cases count: {Cases.Count}");
                        return Page();
                    }

                    var insertCmd = new SqlCommand(
                        "INSERT INTO Cases (CustomerID, CustomerName, Contact, CaseChannel, Description, Status, EmpID, CreatedAt, CreatedBy) " +
                        "VALUES (@CustomerID, @CustomerName, @Contact, @CaseChannel, @Description, @Status, @EmpID, @CreatedAt, @CreatedBy)",
                        connection);
                    insertCmd.Parameters.AddWithValue("@CustomerID", NewCase.CustomerID);
                    insertCmd.Parameters.AddWithValue("@CustomerName", NewCase.CustomerName);
                    insertCmd.Parameters.AddWithValue("@Contact", NewCase.Contact);
                    insertCmd.Parameters.AddWithValue("@CaseChannel", NewCase.CaseChannel);
                    insertCmd.Parameters.AddWithValue("@Description", NewCase.Description);
                    insertCmd.Parameters.AddWithValue("@Status", NewCase.Status);
                    insertCmd.Parameters.AddWithValue("@EmpID", NewCase.EmpID);
                    insertCmd.Parameters.AddWithValue("@CreatedAt", NewCase.CreatedAt);
                    insertCmd.Parameters.AddWithValue("@CreatedBy", NewCase.CreatedBy);

                    Console.WriteLine("DEBUG: Executing insert command");
                    DebugMessage += " | Executing insert command";
                    int rowsAffected = insertCmd.ExecuteNonQuery();
                    Console.WriteLine($"DEBUG: Case added to database. Rows affected: {rowsAffected}");
                    DebugMessage += $" | Case added to database. Rows affected: {rowsAffected}";

                    if (rowsAffected == 0)
                    {
                        Console.WriteLine("DEBUG: Insert failed: No rows affected");
                        DebugMessage += " | Insert failed: No rows affected";
                        ModelState.AddModelError("", "Failed to add case to database.");
                        ViewData["Action"] = "Add";
                        LoadCases();
                        Console.WriteLine($"DEBUG: Returning Page due to insert failure, Cases count: {Cases.Count}");
                        return Page();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Database Error in OnPostAdd: {ex.Message}");
                Console.WriteLine($"DEBUG: Stack Trace: {ex.StackTrace}");
                DebugMessage += $" | Database Error: {ex.Message}";
                ModelState.AddModelError("", $"Failed to add case: {ex.Message}");
                ViewData["Action"] = "Add";
                LoadCases();
                Console.WriteLine($"DEBUG: Returning Page due to database error, Cases count: {Cases.Count}");
                return Page();
            }

            Console.WriteLine("DEBUG: Redirecting to GeneralDashboard after successful add");
            DebugMessage = "Case added successfully!";
            return RedirectToPage("/GeneralDashboard");
        }

        public IActionResult OnPostDelete([FromForm] Guid customerId)
        {
            Console.WriteLine($"DEBUG: OnPostDelete handler invoked with customerId: {customerId}");
            DebugMessage = $"OnPostDelete invoked with customerId: {customerId}";

            if (customerId == Guid.Empty)
            {
                Console.WriteLine("DEBUG: Invalid customerId: Empty GUID received");
                DebugMessage += " | Invalid customerId: Empty GUID received";
                ModelState.AddModelError("", "Invalid Customer ID.");
                LoadCases();
                Console.WriteLine($"DEBUG: Returning Page due to invalid customerId, Cases count: {Cases.Count}");
                return Page();
            }

            Console.WriteLine("DEBUG: Attempting database delete");
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    Console.WriteLine("DEBUG: Database connection opened");
                    DebugMessage += " | Database connection opened";

                    var checkCaseCmd = new SqlCommand("SELECT COUNT(1) FROM Cases WHERE CustomerID = @CustomerID", connection);
                    checkCaseCmd.Parameters.AddWithValue("@CustomerID", customerId);
                    int caseExists = (int)checkCaseCmd.ExecuteScalar();
                    Console.WriteLine($"DEBUG: Case exists check for CustomerID {customerId}: {caseExists}");
                    DebugMessage += $" | Case exists check for CustomerID {customerId}: {caseExists}";

                    if (caseExists == 0)
                    {
                        Console.WriteLine($"DEBUG: Case not found for customerId: {customerId}");
                        DebugMessage += $" | Case not found for customerId: {customerId}";
                        ModelState.AddModelError("", "Case not found.");
                        LoadCases();
                        Console.WriteLine($"DEBUG: Returning Page due to case not found, Cases count: {Cases.Count}");
                        return Page();
                    }

                    var deleteCmd = new SqlCommand("DELETE FROM Cases WHERE CustomerID = @CustomerID", connection);
                    deleteCmd.Parameters.AddWithValue("@CustomerID", customerId);

                    Console.WriteLine("DEBUG: Executing delete command");
                    DebugMessage += " | Executing delete command";
                    int rowsAffected = deleteCmd.ExecuteNonQuery();
                    Console.WriteLine($"DEBUG: Case deleted from database. Rows affected: {rowsAffected}");
                    DebugMessage += $" | Case deleted from database. Rows affected: {rowsAffected}";

                    if (rowsAffected == 0)
                    {
                        Console.WriteLine("DEBUG: Delete failed: No rows affected");
                        DebugMessage += " | Delete failed: No rows affected";
                        ModelState.AddModelError("", "Failed to delete case from database.");
                        LoadCases();
                        Console.WriteLine($"DEBUG: Returning Page due to delete failure, Cases count: {Cases.Count}");
                        return Page();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Database Error in OnPostDelete: {ex.Message}");
                Console.WriteLine($"DEBUG: Stack Trace: {ex.StackTrace}");
                DebugMessage += $" | Database Error: {ex.Message}";
                ModelState.AddModelError("", $"Failed to delete case: {ex.Message}");
                LoadCases();
                Console.WriteLine($"DEBUG: Returning Page due to database error, Cases count: {Cases.Count}");
                return Page();
            }

            Console.WriteLine("DEBUG: Redirecting to GeneralDashboard after successful delete");
            DebugMessage = "Case deleted successfully!";
            return RedirectToPage("/GeneralDashboard");
        }

        public IActionResult OnPostLogout()
        {
            Console.WriteLine("DEBUG: OnPostLogout handler invoked");
            DebugMessage = "Logout requested";

            try
            {
                // Clear the session
                HttpContext.Session.Clear();
                Console.WriteLine("DEBUG: Session cleared successfully");
                DebugMessage += " | Session cleared successfully";

                // Redirect to login page
                return Redirect("~/login.html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error during logout: {ex.Message}");
                DebugMessage += $" | Error during logout: {ex.Message}";
                ModelState.AddModelError("", "Error occurred during logout");
                LoadCases();
                return Page();
            }
        }

        private void LoadCases()
        {
            Console.WriteLine("DEBUG: LoadCases called");
            DebugMessage += " | LoadCases called";
            Cases.Clear();
            Console.WriteLine("DEBUG: Cases list cleared");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    Console.WriteLine("DEBUG: Attempting to open database connection");
                    connection.Open();
                    Console.WriteLine("DEBUG: Database connection opened for LoadCases");
                    DebugMessage += " | Database connection opened for LoadCases";

                    var query = "SELECT CustomerID, CustomerName, Contact, CaseChannel, Description, Status, EmpID, CreatedAt, CreatedBy FROM Cases";
                    if (!string.IsNullOrEmpty(FilterName))
                    {
                        query += " WHERE CustomerName LIKE @FilterName";
                    }
                    if (!string.IsNullOrEmpty(FilterChannel))
                    {
                        query += string.IsNullOrEmpty(FilterName) ? " WHERE" : " AND";
                        query += " CaseChannel = @FilterChannel";
                    }

                    var cmd = new SqlCommand(query, connection);
                    if (!string.IsNullOrEmpty(FilterName))
                    {
                        cmd.Parameters.AddWithValue("@FilterName", "%" + FilterName + "%");
                        Console.WriteLine($"DEBUG: Applying filter: CustomerName LIKE '%{FilterName}%'");
                        DebugMessage += $" | Applying filter: CustomerName LIKE '%{FilterName}%'";
                    }
                    if (!string.IsNullOrEmpty(FilterChannel))
                    {
                        cmd.Parameters.AddWithValue("@FilterChannel", FilterChannel);
                        Console.WriteLine($"DEBUG: Applying filter: CaseChannel = '{FilterChannel}'");
                        DebugMessage += $" | Applying filter: CaseChannel = '{FilterChannel}'";
                    }

                    Console.WriteLine($"DEBUG: Executing query: {query}");
                    DebugMessage += " | Executing load cases query";
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("DEBUG: Reading query results");
                        while (reader.Read())
                        {
                            var newCase = new Case
                            {
                                CustomerID = reader.GetGuid(0),
                                CustomerName = reader.GetString(1),
                                Contact = reader.GetString(2),
                                CaseChannel = reader.GetString(3),
                                Description = reader.GetString(4),
                                Status = reader.GetString(5),
                                EmpID = reader.GetGuid(6),
                                CreatedAt = reader.GetDateTime(7),
                                CreatedBy = reader.GetGuid(8)
                            };
                            Cases.Add(newCase);
                            Console.WriteLine($"DEBUG: Loaded case: {newCase.CustomerID}, {newCase.CustomerName}");
                        }
                    }
                    Console.WriteLine($"DEBUG: Loaded {Cases.Count} cases from database");
                    DebugMessage += $" | Loaded {Cases.Count} cases from database";
                    if (Cases.Count == 0)
                    {
                        Console.WriteLine("DEBUG: Warning: No cases loaded from database");
                        DebugMessage += " | Warning: No cases loaded";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error in LoadCases: {ex.Message}");
                Console.WriteLine($"DEBUG: Stack Trace: {ex.StackTrace}");
                DebugMessage += $" | Error in LoadCases: {ex.Message}";
            }
            Console.WriteLine($"DEBUG: LoadCases completed, Cases count: {Cases.Count}");
        }
    }
}