using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;

namespace FinalProject.Pages.Compose_New_Email
{
    [Authorize]
    public class ReplyMailModel : PageModel
    {
        [BindProperty]
        public string To { get; set; }
        [BindProperty]
        public string Subject { get; set; }
        [BindProperty]
        public string Message { get; set; }

        public string ErrorMessage { get; private set; }
        public string SuccessMessage { get; private set; }

        public IActionResult OnGet(int emailid)
        {
            string connectionString = "Server=tcp:cs436final.database.windows.net,1433;Initial Catalog=FinalProject;Persist Security Info=False;User ID=final_admin;Password=Cs436227F;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT emailreceiver FROM emails WHERE emailid = @emailid";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@emailid", emailid);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string emailReceiver = reader.GetString(0);

                            // ตรวจสอบว่าผู้ส่งตรงกับผู้ใช้ที่เข้าสู่ระบบหรือไม่
                            if (!emailReceiver.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                // ถ้าไม่ตรง, กลับไปยังหน้าที่กำหนด
                                return RedirectToPage("/Index"); // ปรับเปลี่ยนเส้นทางไปยังหน้าที่คุณต้องการ
                            }

                            To = emailReceiver; // กำหนดค่า To
                        }
                    }
                }
            }

            // ดำเนินการตามปกติถ้าการตรวจสอบผ่าน
            return Page();
        }


        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(To) || string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Message))
            {
                ErrorMessage = "All fields are required";
                return Page();
            }

            if (To.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "You cannot send an email to yourself.";
                return Page();
            }

            try
            {
                string connectionString = "Server=tcp:cs436final.database.windows.net,1433;Initial Catalog=FinalProject;Persist Security Info=False;User ID=final_admin;Password=Cs436227F;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string userCheckSql = "SELECT COUNT(*) FROM AspNetUsers WHERE UserName= @receiver";
                    using (SqlCommand userCheckCommand = new SqlCommand(userCheckSql, connection))
                    {
                        userCheckCommand.Parameters.AddWithValue("@receiver", To);
                        int userExists = (int)userCheckCommand.ExecuteScalar();
                        if (userExists == 0)
                        {
                            ErrorMessage = "Recipient does not exist.";
                            return Page();
                        }
                    }

                    string sql = "INSERT INTO emails (emailsubject, emailmessage, emaildate, emailisread, emailsender, emailreceiver) VALUES (@subject, @message, @date, @isread, @sender, @receiver)";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@subject", Subject);
                        command.Parameters.AddWithValue("@message", Message);
                        command.Parameters.AddWithValue("@date", DateTime.Now);
                        command.Parameters.AddWithValue("@isread", 0);
                        command.Parameters.AddWithValue("@sender", User.Identity.Name);
                        command.Parameters.AddWithValue("@receiver", To);
                        command.ExecuteNonQuery();
                    }
                }
                SuccessMessage = "Email sent successfully";
                return RedirectToPage("/ReplySuccess");
            }
            catch (SqlException sqlEx)
            {
                ErrorMessage = "Database error: " + sqlEx.Message;
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred: " + ex.Message;
                return Page();
            }
        }
    }
}
