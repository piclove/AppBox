<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="AppBox._default" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>系统登陆</title>
    <script type="text/javascript">

        // 本页面一定是顶层窗口，不会嵌在IFrame中
        if (top.window != window) {
            top.window.location.href = "./default.aspx";
        }

        // 将 localhost 转换为 localhost/default.aspx
        if (window.location.href.indexOf('/default.aspx') < 0) {
            window.location.href = "./default.aspx";
        }

    </script>
    <style>
        .login-image {
            background-color: #efefef;
            border-right: solid 1px #ddd;
        }

            .login-image img {
                width: 160px;
                height: 160px;
                padding: 10px;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <f:PageManager ID="PageManager1" runat="server"></f:PageManager>
        <f:Window ID="Window1" runat="server" IsModal="true" Hidden="false" EnableClose="false"
            EnableMaximize="false" WindowPosition="GoldenSection" Icon="Key" Title="系统登陆"
            Layout="HBox" BoxConfigAlign="Stretch" BoxConfigPosition="Start" Width="500px" Plain="false">
            <Items>
                <f:Image ID="imageLogin" ImageUrl="~/res/images/login/login_2.png" runat="server"
                    CssClass="login-image">
                </f:Image>
                <f:SimpleForm ID="SimpleForm1" LabelAlign="Top" BoxFlex="1" runat="server" LabelWidth="45px"
                    BodyPadding="30px 20px" ShowBorder="false" ShowHeader="false">
                    <Items>
                        <f:TextBox ID="tbxUserName" FocusOnPageLoad="true" runat="server" Label="帐号" Required="true"
                            ShowRedStar="true" Text="">
                        </f:TextBox>
                        <f:TextBox ID="tbxPassword" TextMode="Password" runat="server" Required="true" ShowRedStar="true"
                            Label="密码" Text="">
                        </f:TextBox>
                    </Items>
                </f:SimpleForm>
            </Items>
            <Toolbars>
                <f:Toolbar runat="server" Position="Footer" ToolbarAlign="Right">
                    <Items>
                        <f:Button ID="btnSubmit" Icon="LockOpen" Type="Submit" runat="server" ValidateForms="SimpleForm1"
                            OnClick="btnSubmit_Click" Text="登陆">
                        </f:Button>
                    </Items>
                </f:Toolbar>
            </Toolbars>
        </f:Window>
    </form>
</body>
</html>
