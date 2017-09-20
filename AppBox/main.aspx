<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="main.aspx.cs" Inherits="AppBox.main" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>首页</title>
    <link href="res/css/main.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <f:PageManager ID="PageManager1" AutoSizePanelID="regionPanel" runat="server" />
        <f:RegionPanel ID="regionPanel" ShowBorder="false" runat="server">
            <Regions>
                <f:Region ID="regionTop" ShowBorder="false" ShowHeader="false" Position="Top"
                    Layout="Fit" runat="server">
                    <Toolbars>
                        <f:Toolbar ID="Toolbar1" Position="Bottom" runat="server" CssClass="topbar">
                            <Items>
                                <f:ToolbarText ID="txtUser" runat="server">
                                </f:ToolbarText>
                                <f:ToolbarSeparator runat="server" />
                                <f:ToolbarText ID="txtOnlineUserCount" runat="server">
                                </f:ToolbarText>
                                <f:ToolbarSeparator runat="server" />
                                <f:ToolbarText ID="txtCurrentTime" runat="server">
                                </f:ToolbarText>
                                <f:ToolbarFill runat="server" />
                                <f:Button ID="btnRefresh" runat="server" Icon="Reload" ToolTip="刷新主区域内容" EnablePostBack="false">
                                </f:Button>
                                <f:ToolbarSeparator runat="server" />
                                <f:Button ID="btnHelp" EnablePostBack="false" Icon="Help" Text="帮助" runat="server">
                                </f:Button>
                                <f:ToolbarSeparator runat="server" />
                                <f:Button ID="btnExit" runat="server" Icon="UserRed" Text="安全退出" ConfirmText="确定退出系统？"
                                    OnClick="btnExit_Click">
                                </f:Button>
                            </Items>
                        </f:Toolbar>
                    </Toolbars>
                </f:Region>
                <f:Region ID="regionLeft" Split="true"
                    EnableCollapse="true" Width="200px"
                    ShowHeader="true" Title="系统菜单" Layout="Fit" Position="Left" runat="server">
                </f:Region>
                <f:Region ID="mainRegion" ShowHeader="false" Layout="Fit" Margins="0 0 0 0" Position="Center"
                    runat="server">
                    <Items>
                        <f:TabStrip ID="mainTabStrip" EnableTabCloseMenu="true" ShowBorder="false" runat="server">
                            <Tabs>
                                <f:Tab ID="Tab1" Title="首页" EnableIFrame="true" IFrameUrl="~/admin/default.aspx"
                                    Icon="House" runat="server">
                                </f:Tab>
                            </Tabs>
                        </f:TabStrip>
                    </Items>
                </f:Region>
            </Regions>
        </f:RegionPanel>
        <f:Window ID="Window1" runat="server" IsModal="true" Hidden="true" EnableIFrame="true"
            EnableResize="true" EnableMaximize="true" IFrameUrl="about:blank" Width="650px"
            Height="450px">
        </f:Window>
    </form>
    <script src="res/js/main.js" type="text/javascript"></script>
</body>
</html>
