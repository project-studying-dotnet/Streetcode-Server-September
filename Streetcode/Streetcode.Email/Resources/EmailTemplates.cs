namespace Streetcode.Email.Resources
{
    public static class EmailTemplates
    {
        public static Func<string, string, string> GetRegistrationEmailTemplate = (userName, websiteLink) => $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome to Streetcode</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            width: 100%;
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }}
        h1 {{
            color: #333;
        }}
        p {{
            font-size: 16px;
            color: #555;
        }}
        .button {{
            display: inline-block;
            padding: 10px 20px;
            margin-top: 20px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 4px;
        }}
        .footer {{
            margin-top: 30px;
            font-size: 12px;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>Welcome to Streetcode!</h1>
        <p>Hi {userName},</p>
        <p>Thank you for registering on Streetcode. We are excited to have you as part of our community. You can now explore all the features and resources available on our platform.</p>
        <p>If you have any questions or need assistance, feel free to reach out to our support team at any time.</p>
        <a href=""{websiteLink}"" class=""button"" style=""color : white !important"">Get Started</a>
        <p class=""footer"">If you did not register for this account, please ignore this email or contact our support.</p>
        <p class=""footer"">Best regards,<br>Streetcode Team</p>
    </div>
</body>
</html>";
    }
}
