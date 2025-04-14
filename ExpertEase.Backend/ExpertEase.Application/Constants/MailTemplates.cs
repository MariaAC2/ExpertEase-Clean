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
                <h2 style=""color: #333;"">Bun venit pe platforma ExpertEase, {name}!</h2>
                <p style=""font-size: 16px; color: #555;"">
                    Contul dumeavoastra a fost creat cu succes!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Acum puteti beneficia de pe urma serviciilor noastre de cea mai buna calitate. 

                    Pentru mai multe intrebari, nu ezitati sa ne contactati!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Multumim,<br/>
                    Echipa ExpertEase
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
                <h2 style=""color: #333;"">Bun venit in echipa ExpertEase, {name}!</h2>
                <p style=""font-size: 16px; color: #555;"">
                    Acum faceti parte din echipa de specialisti din cadrul aplicatiei ExpertEase!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Acum puteti sa va modificati datele, sa accesati solicitari si sa trimiteti oferte clientilor dumneavoastra.
                    De asemenea, din contul ExpertEase, puteti retrage suma incasata de pe urma serviciilor oferite.

                    Pentru mai multe intrebari, nu ezitati sa ne contactati!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Multumim,<br/>
                    Echipa ExpertEase
                </p>
            </div>
        </body>
</html>";
    
    public static string TransactionRejectedTemplate(string name) => $@"<!DOCTYPE html>
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
                <h2 style=""color: #333;"">Bun venit in echipa ExpertEase, {name}!</h2>
                <p style=""font-size: 16px; color: #555;"">
                    Acum faceti parte din echipa de specialisti din cadrul aplicatiei ExpertEase!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Acum puteti sa va modificati datele, sa accesati solicitari si sa trimiteti oferte clientilor dumneavoastra.
                    De asemenea, din contul ExpertEase, puteti retrage suma incasata de pe urma serviciilor oferite.

                    Pentru mai multe intrebari, nu ezitati sa ne contactati!
                </p>

                <p style=""font-size: 16px; color: #555;"">
                    Multumim,<br/>
                    Echipa ExpertEase
                </p>
            </div>
        </body>
</html>";
}
