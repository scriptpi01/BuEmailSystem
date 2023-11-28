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
        public string EmailReceiver { get; set; }
        public int EmailId { get; set; }

        public IActionResult OnGet(int emailid)
        {
            if (string.IsNullOrEmpty(User.Identity.Name))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            EmailId = emailid;
            if (!LoadEmail(emailid))
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        private bool LoadEmail(int emailid)
        {
            string connectionString = "Server=tcp:cs436final.database.windows.net,1433;Initial Catalog=FinalProject;Persist Security Info=False;User ID=final_admin;Password=Cs436227F;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string selectSql = "SELECT emailsender, emailsubject, emailmessage, emailreceiver FROM emails WHERE emailid = @emailid";
                using (SqlCommand selectCommand = new SqlCommand(selectSql, connection))
                {
                    selectCommand.Parameters.AddWithValue("@emailid", emailid);
                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            EmailSender = reader.GetString(0);
                            EmailSubject = reader.GetString(1);
                            EmailMessage = reader.GetString(2);
                            EmailReceiver = reader.GetString(3);

                            if (EmailReceiver != User.Identity.Name)
                            {
                                return false;
                            }

                            // Mark the email as read
                            reader.Close();
                            string updateSql = "UPDATE emails SET emailisread = 1 WHERE emailid = @emailid";
                            using (SqlCommand updateCommand = new SqlCommand(updateSql, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@emailid", emailid);
                                updateCommand.ExecuteNonQuery();
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
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

                return RedirectToPage("/DeleteSuccess");
            }
            catch (Exception ex)
            {
                // Handle exception
                // Log the exception, show an error message, etc.
                return Page();
            }
        }
    }
}
