<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PowershellVw.aspx.cs" Inherits="PowershellScriptConsumeMVC.Views.Home.PowershellVw" %>

<!DOCTYPE html>
 
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Get-ChildItem</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div><h1 align="center">Get-ChildItem (itdroplets.com)</h1></div>
            <p>Please type the directory for which you want to get the child items:
                <asp:TextBox ID="Input" runat="server" Width="100%" Height="20px" ></asp:TextBox>
            </p>
            <asp:Button ID="ExecuteInput" runat="server" Text="Execute" Width="200" onclick="ExecuteInputClick" />
 
            <p>Result
            <asp:TextBox ID="Result" TextMode="MultiLine" Width="100%" Height="450px" runat="server"></asp:TextBox>
            </p>
 
        </div>
    </form>
</body>
</html>