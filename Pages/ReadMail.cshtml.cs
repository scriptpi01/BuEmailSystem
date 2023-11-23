using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;

namespace FinalProject.Pages
{
    public class ReadMailModel : PageModel
    {
        public string EmailSender { get; set; }
        public string EmailSubject { get; set; }
        public string EmailMessage { get; set; }
        public int EmailId { get; set; }

        public void OnGet(int emailid)
        {
            EmailId = emailid; // Store the email ID for use in the page
            string connectionString = "Server=tcp:cs436final.database.windows.net,1433;Initial Catalog=FinalProject;Persist Security Info=False;User ID=final_admin;Password=Cs436227F;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Update the email as read
                string updateSql = "UPDATE emails SET emailisread = 1 WHERE emailid = @emailid";
                using (SqlCommand updateCommand = new SqlCommand(updateSql, connection))
                {
                    updateCommand.Parameters.AddWithValue("@emailid", emailid);
                    updateCommand.ExecuteNonQuery();
                }

                // Retrieve the email details
                string selectSql = "SELECT emailsender, emailsubject, emailmessage FROM emails WHERE emailid = @emailid";
                using (SqlCommand selectCommand = new SqlCommand(selectSql, connection))
                {
                    selectCommand.Parameters.AddWithValue("@emailid", emailid);
                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            EmailSender = reader.GetString(0);
                            EmailSubject = reader.GetString(1);
                            EmailMessage = "\n" + reader.GetString(2); // Prepend a newline character
                        }
                    }
                }
            }
        }


        public IActionResult OnPostDelete(int emailId)
        {
            try
            {
                string connectionString = "Server = tcp:cs436final.database.windows.net,1433; Initial Catalog = FinalProject; Persist Security Info = False; User ID = final_admin; Password = Cs436227F; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30; ";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "DELETE FROM emails WHERE emailid=@emailId";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@emailId", emailId);
                        command.ExecuteNonQuery();
                    }
                }

                // Optionally add a success message or redirect
                return RedirectToPage("/DeleteSuccess");
            }
            catch (Exception ex)
            {
                // Handle the error, maybe display a message
                return Page();
            }
        }

    }
}
