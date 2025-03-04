function closeModal() {
    const modal = document.getElementById("editCaseModal");
    console.log("Closing modal...");
    modal.classList.remove("show");
    modal.style.display = "none";
} document.addEventListener("DOMContentLoaded", function () {
    console.log("script.js loaded and DOMContentLoaded fired");

    const caseForm = document.getElementById("caseForm");
    const caseTableBody = document.getElementById("caseTableBody");
    const saveEditBtn = document.getElementById("saveEditBtn");
    const loginForm = document.getElementById("loginForm");
    const closeModalBtn = document.getElementById("closeModalBtn");
    const closeBtn = document.querySelector(".btn-close");
    const logoutBtn = document.getElementById("logoutBtn");

    console.log("Checking DOM elements:");
    console.log("caseForm:", caseForm ? "found" : "not found");
    console.log("caseTableBody:", caseTableBody ? "found" : "not found");
    console.log("saveEditBtn:", saveEditBtn ? "found" : "not found");
    console.log("loginForm:", loginForm ? "found" : "not found");
    console.log("closeModalBtn:", closeModalBtn ? "found" : "not found");
    console.log("closeBtn:", closeBtn ? "found" : "not found");
    console.log("logoutBtn:", logoutBtn ? "found" : "not found");

    async function fetchWhatsappCases() {
        try {
            const response = await fetch("http://localhost:5198/api/cases/whatsapp");
            if (!response.ok) {
                throw new Error(`API error: ${response.status}`);
            }
            const cases = await response.json();
            console.log("Fetched WhatsApp Cases:", cases);

            const tableBody = document.getElementById("caseTableBody");
            tableBody.innerHTML = "";
            cases.forEach(caseData => {
                console.log("Rendering case:", caseData);
                const row = document.createElement("tr");
                row.innerHTML = `
                <td data-label="Case ID">${caseData.customerID}</td>
                <td data-label="Customer Name">${caseData.customerName}</td>
                <td data-label="Contact">${caseData.contact}</td>
                <td data-label="Channel">${caseData.caseChannel}</td>
                <td data-label="Description">${caseData.description}</td>
                <td data-label="Status">${caseData.status}</td>
                <td data-label="Created By">${caseData.createdBy}</td>
                <td data-label="Created At">${new Date(caseData.createdAt).toLocaleString()}</td>
                <td data-label="Options">
                    <button class="edit-btn" data-id="${caseData.customerID}">Edit</button>
                    <button class="delete-btn" data-id="${caseData.customerID}">Delete</button>
                </td>
            `;
                tableBody.appendChild(row);
            });
        } catch (error) {
            console.error("Error fetching WhatsApp cases:", error);
        }
    }

    fetchWhatsappCases();

    caseForm?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const empID = document.getElementById("empID").value.trim();
        if (!validateGuid(empID)) {
            alert("Invalid Employee ID format. It must be a valid GUID.");
            return;
        }
        const newCase = {
            empID,
            createdBy: empID,
            customerName: document.getElementById("customerName").value.trim(),
            contact: document.getElementById("contact").value.trim(),
            caseChannel: document.getElementById("caseChannel").value.trim(),
            description: document.getElementById("description").value.trim(),
            status: "Open",
            createdAt: new Date().toISOString(),
        };
        try {
            const response = await fetch("http://localhost:5198/api/cases", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(newCase),
            });
            if (!response.ok) throw new Error("Failed to add case.");
            caseForm.reset();
            fetchWhatsappCases();
        } catch (error) {
            alert("An error occurred while adding the case.");
            console.error("Add Case Error:", error);
        }
    });

    if (loginForm) {
        loginForm.addEventListener("submit", function (event) {
            event.preventDefault();
            const username = document.getElementById("username").value;
            const password = document.getElementById("password").value;

            fetch("http://localhost:5198/Login/Authenticate", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, password })
            })
                .then(response => response.json())
                .then(data => {
                    console.log("Server Response:", data);
                    if (data.role) {
                        const role = data.role.toLowerCase();
                        console.log("Role detected:", role);
                        if (role === "whatsappteam") {
                            console.log("Redirecting to whatsapp-dashboard.html");
                            window.location.href = "whatsapp-dashboard.html";
                        } else if (role === "customersupport") {
                            console.log("Redirecting to /GeneralDashboard");
                            window.location.href = "/GeneralDashboard";
                        } else {
                            console.log("Unknown role:", role);
                            document.getElementById("errorMessage").textContent = "Unknown role!";
                        }
                    } else {
                        document.getElementById("errorMessage").textContent = "Invalid credentials!";
                    }
                })
                .catch(error => {
                    console.error("Error:", error);
                    document.getElementById("errorMessage").textContent = "Login failed. Please try again.";
                });
        });
    }

    caseTableBody.addEventListener("click", async function (event) {
        if (event.target.classList.contains("edit-btn")) {
            const customerID = event.target.getAttribute("data-id");
            try {
                const response = await fetch(`http://localhost:5198/api/cases/${customerID}`);
                if (!response.ok) throw new Error("Failed to fetch case.");
                const caseData = await response.json();
                openEditModal(caseData);
            } catch (error) {
                console.error("Error fetching case details:", error);
            }
        }

        if (event.target.classList.contains("delete-btn")) {
            const customerID = event.target.getAttribute("data-id");
            if (!confirm("Are you sure?")) return;
            try {
                const response = await fetch(`http://localhost:5198/api/cases/${customerID}`, { method: "DELETE" });
                if (!response.ok) throw new Error("Failed to delete case.");
                fetchWhatsappCases();
            } catch (error) {
                console.error("Error deleting case:", error);
            }
        }
    });

    function openEditModal(caseData) {
        console.log("Opening modal with case:", caseData);
        document.getElementById("editCustomerID").value = caseData.customerID;
        document.getElementById("editCustomerName").value = caseData.customerName;
        document.getElementById("editContact").value = caseData.contact;
        document.getElementById("editCaseChannel").value = caseData.caseChannel;
        document.getElementById("editDescription").value = caseData.description;
        document.getElementById("editStatus").value = caseData.status;
        document.getElementById("editEmpID").value = caseData.empID;
        document.getElementById("editCreatedBy").value = caseData.createdBy;

        document.getElementById("editCreatedBy").setAttribute("readonly", "true");
        document.getElementById("editEmpID").setAttribute("readonly", "true");

        const modal = document.getElementById("editCaseModal");
        modal.classList.add("show");
        modal.style.display = "block";
    }

    saveEditBtn.addEventListener("click", async () => {
        const customerID = document.getElementById("editCustomerID").value.trim();
        const updatedCase = {
            customerID: customerID,
            customerName: document.getElementById("editCustomerName").value.trim(),
            contact: document.getElementById("editContact").value.trim(),
            caseChannel: document.getElementById("editCaseChannel").value.trim(),
            description: document.getElementById("editDescription").value.trim(),
            status: document.getElementById("editStatus").value.trim(),
            createdBy: document.getElementById("editCreatedBy").value.trim()
        };

        console.log("Sending updated case to API:", updatedCase);

        try {
            const response = await fetch(`http://localhost:5198/api/cases/${customerID}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(updatedCase),
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Update failed: ${response.status} - ${errorText}`);
            }

            console.log("Case updated successfully!");
            fetchWhatsappCases();
            closeModal();
        } catch (error) {
            console.error("Error updating case:", error);
            alert("Failed to update case: " + error.message);
        }
    });

    if (closeModalBtn) {
        closeModalBtn.addEventListener("click", function () {
            console.log("Close button (footer) clicked");
            closeModal();
        });
    } else {
        console.error("closeModalBtn not found in DOM");
    }
    if (closeBtn) {
        closeBtn.addEventListener("click", function () {
            console.log("Close button (×) clicked");
            closeModal();
        });
    } else {
        console.error("closeBtn not found in DOM");
    }

    if (logoutBtn) {
        console.log("Attaching event listener to logoutBtn");
        logoutBtn.addEventListener("click", function () {
            console.log("Logout button clicked on WhatsApp dashboard");
            window.location.href = "/login.html";
        });
    } else {
        console.error("logoutBtn not found in DOM when script.js runs");
    }


    document.addEventListener("DOMContentLoaded", function () {
        
        const logoutBtn = document.getElementById("logoutBtn");

        if (logoutBtn) {
            logoutBtn.addEventListener("click", function () {
                console.log("Logout button clicked");

                window.location.href = "/login.html";
            });
            console.log("Logout button event listener attached");
        } else {
            console.error("Logout button not found in the DOM");
        }
    });

    function validateGuid(guid) {
        const guidRegex = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;
        return guidRegex.test(guid);
    }

});

