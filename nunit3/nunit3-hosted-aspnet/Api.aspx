<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Api.aspx.cs" Inherits="NUnit.Hosted.AspNet.Api1" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Results</title>
    <link href="Content/nunit-hosted.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="nunit">
            <h1 id="nunit-header"><%= HeaderTitle %></h1>
            <% if (Failure) { %>
            <h2 id="nunit-banner" class="nunit-fail"></h2>
            <%}else { %>
            <h2 id="nunit-banner" class="nunit-pass"></h2>
            <%} %>
            <p id="nunit-testresult" class="result">Tests completed.<br>
                <span class="passed"><%= Passed %></span> assertions of <span class="total"><%= Total %></span> passed, <span class="failed"><%= Failed %></span> failed.</p>
            <ol id="nunit-tests">
                <%= RenderedResults %>
            </ol>
        </div>
    </form>
    <script type="text/javascript">
        var msgs = '<%=HttpUtility.JavaScriptStringEncode(ConsoleOut)%>'.split(/[\n\r]+/);
        for (var i = 0; i < msgs.length; i++) {
            console.log(msgs[i]);
        }
    </script>
</body>
</html>
