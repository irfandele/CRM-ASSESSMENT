using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using CRM.Models.Entities;

namespace CRM.Pages
{
    [ValidateAntiForgeryToken]
    public class EditModel : PageModel
    {
        private readonly string _connectionString;

        public EditModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        [BindProperty]
        public Case? Case { get; set; }

        public IActionResult OnGet(Guid customerID)
        {
            Console.WriteLine($"DEBUG: OnGet called: customerID={customerID}");
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    Console.WriteLine("DEBUG: Database connection opened for OnGet");

                    var cmd = new SqlCommand(
                        "SELECT CustomerID, CustomerName, Contact, CaseChannel, Description, Status, EmpID, CreatedAt, CreatedBy " +
                        "FROM Cases WHERE CustomerID = @CustomerID", connection);
                    cmd.Parameters.AddWithValue("@CustomerID", customerID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Case = new Case
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
                            Console.WriteLine($"DEBUG: Loaded case: {Case.CustomerID}, {Case.CustomerName}");
                        }
                        else
                        {
                            Console.WriteLine($"DEBUG: Case not found for customerID: {customerID}");
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error in OnGet: {ex.Message}");
                Console.WriteLine($"DEBUG: Stack Trace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred while loading the case.");
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            Console.WriteLine($"DEBUG: OnPost called: CustomerID={Case?.CustomerID}, CustomerName={Case?.CustomerName}");

            if (!ModelState.IsValid || Case == null)
            {
                Console.WriteLine("DEBUG: ModelState invalid or Case is null");
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"DEBUG: Validation Error: {entry.Key} - {error.ErrorMessage}");
                    }
                }
                return Page();
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    Console.WriteLine("DEBUG: Database connection opened for OnPost");

                    var checkCmd = new SqlCommand(
                        "SELECT EmpID, CreatedBy, CreatedAt FROM Cases WHERE CustomerID = @CustomerID",
                        connection);
                    checkCmd.Parameters.AddWithValue("@CustomerID", Case.CustomerID);

                    using (var reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Case.EmpID = reader.GetGuid(0);
                            Case.CreatedBy = reader.GetGuid(1);
                            Case.CreatedAt = reader.GetDateTime(2);
                            Console.WriteLine($"DEBUG: Preserved case data: EmpID={Case.EmpID}, CreatedBy={Case.CreatedBy}, CreatedAt={Case.CreatedAt}");
                        }
                        else
                        {
                            Console.WriteLine($"DEBUG: Case not found for CustomerID: {Case.CustomerID}");
                            ModelState.AddModelError("", "Case not found.");
                            return Page();
                        }
                    }

                    var updateCmd = new SqlCommand(
                        "UPDATE Cases SET CustomerName = @CustomerName, Contact = @Contact, CaseChannel = @CaseChannel, " +
                        "Description = @Description, Status = @Status WHERE CustomerID = @CustomerID", connection);
                    updateCmd.Parameters.AddWithValue("@CustomerName", Case.CustomerName);
                    updateCmd.Parameters.AddWithValue("@Contact", Case.Contact);
                    updateCmd.Parameters.AddWithValue("@CaseChannel", Case.CaseChannel);
                    updateCmd.Parameters.AddWithValue("@Description", Case.Description);
                    updateCmd.Parameters.AddWithValue("@Status", Case.Status);
                    updateCmd.Parameters.AddWithValue("@CustomerID", Case.CustomerID);

                    Console.WriteLine("DEBUG: Executing update command");
                    int rowsAffected = updateCmd.ExecuteNonQuery();
                    Console.WriteLine($"DEBUG: Case updated successfully. Rows affected: {rowsAffected}");

                    if (rowsAffected == 0)
                    {
                        Console.WriteLine("DEBUG: Update failed: No rows affected");
                        ModelState.AddModelError("", "Failed to update case in database.");
                        return Page();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error in OnPost: {ex.Message}");
                Console.WriteLine($"DEBUG: Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Failed to update case: {ex.Message}");
                return Page();
            }

            Console.WriteLine("DEBUG: Redirecting to GeneralDashboard after successful update");
            return RedirectToPage("/GeneralDashboard");
        }
    }
}