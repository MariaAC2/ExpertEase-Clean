namespace ExpertEase.Application.Constants;

/// <summary>
/// Here we have a class that provides HTML template for mail bodies. You ami add or change the template if you like.
/// </summary>
public static class MailTemplates
{
    public static string UserAddTemplate(string name) => $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>Mail</title>
    <style type=""text/css"">
        p {{
            margin: 0 0 5px 0;
        }}
    </style>
</head>
        <body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">
            <div style=""max-width: 600px; margin: auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);"">
                <h2 style=""color: #333;"">Welcome to ExpertEase, {name}!</h2>
                <p style=""font-size: 16px; color: #555;"">
                    Your account was created successfully!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Now you can access your profile, view your requests and manage your transactions.
                    If you have any questions, feel free to contact us!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Thank you,<br/>
                    ExpertEase Team
                </p>
            </div>
        </body>
</html>";
    
    public static string SpecialistAddTemplate(string name) => $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>Mail</title>
    <style type=""text/css"">
        p {{
            margin: 0 0 5px 0;
        }}
    </style>
</head>
        <body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">
            <div style=""max-width: 600px; margin: auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);"">
                <h2 style=""color: #333;"">Congratulations! You became an ExpertEase specialist, {name}!</h2>
                <p style=""font-size: 16px; color: #555;"">
                    Now you are part of our select team of specialists!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Now you can modify your profile, access requests and send offers to your clients.
                    You can also withdraw the amount earned from the services you provided.

                    If you have any questions, feel free to contact us!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Thank you,<br/>
                    ExpertEase Team
                </p>
            </div>
        </body>
</html>";
    
    public static string TransactionAddTemplate(string name, string transactionType, string summary) => $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>Mail</title>
    <style type=""text/css"">
        p {{
            margin: 0 0 5px 0;
        }}
    </style>
</head>
        <body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">
            <div style=""max-width: 600px; margin: auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);"">
                <h2 style=""color: #333;"">Your ExpertEase transaction, {name}!</h2>
                <p style=""font-size: 16px; color: #555;"">
                    Your transaction of type {transactionType} was added!
                    Now, you have to wait for the admin to approve it.
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Transaction summary:
                    {summary}
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Thank you,<br/>
                    ExpertEase Team
                </p>
            </div>
        </body>
</html>";
    
    public static string TransactionInvalidTemplate(string name, string summary) => $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>Mail</title>
    <style type=""text/css"">
        p {{
            margin: 0 0 5px 0;
        }}
    </style>
</head>
        <body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">
            <div style=""max-width: 600px; margin: auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);"">
                <h2 style=""color: #333;"">Your ExpertEase transaction, {name}!</h2>
                <p style=""font-size: 16px; color: #555;"">
                    Your transaction was invalidated!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Transaction summary:
                    {summary}
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Thank you,<br/>
                    ExpertEase Team
                </p>
            </div>
        </body>
</html>";
    
    public static string TransactionProcessedTemplate(string name, string transactionType, string status, string summary) => $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>Mail</title>
    <style type=""text/css"">
        p {{
            margin: 0 0 5px 0;
        }}
    </style>
</head>
        <body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">
            <div style=""max-width: 600px; margin: auto; background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);"">
                <h2 style=""color: #333;"">Your ExpertEase transaction, {name}!</h2>
                <p style=""font-size: 16px; color: #555;"">
                    Your transaction of type {transactionType} was processed!
                    Status: {status}
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Transaction summary:
                    {summary}
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Thank you,<br/>
                    ExpertEase Team
                </p>
            </div>
        </body>
</html>";
}


