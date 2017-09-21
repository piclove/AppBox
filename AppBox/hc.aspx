<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="hc.aspx.cs" Inherits="AppBox.hc" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <link href="../res/css/common.css" rel="stylesheet" type="text/css" />
    <style type="text/css">
        .x-grid-item.highlight td {
            background-color: lightgreen;
            background-image: none;
        }

        .x-grid-item-selected.highlight td {
            background-color: yellow;
            background-image: none;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <f:PageManager ID="PageManager1" AutoSizePanelID="RegionPanel1"  runat="server"></f:PageManager>
        <f:RegionPanel ID="RegionPanel1" runat="server" ShowBorder="True" AutoScroll="true"  >
        <Regions>
            <f:Region runat="server" ID="panelCenterRegion" Position="Center" Split="true" EnableCollapse="true" Layout="Fit" 
                Title="航材库存清单" ShowBorder="true" ShowHeader="true" BodyPadding="5px">
                <Toolbars>
                    <f:Toolbar runat="server">
                        <Items>
                            <f:TwinTriggerBox ID="ttbSearchRo" runat="server" ShowLabel="false" EmptyText="使用部件名称或序号搜索" Trigger1Icon="Clear" Trigger2Icon="Search" ShowTrigger1="false" OnTrigger2Click="ttbSearchRo_Trigger2Click" OnTrigger1Click="ttbSearchRo_Trigger1Click">
                            </f:TwinTriggerBox>
                            <f:Label ID="Label2" runat="server">
                            </f:Label>
                        </Items>
                    </f:Toolbar>
                </Toolbars>
                <Items>
                    <f:Grid ID="Grid1" runat="server" ShowBorder="true" ShowHeader="false" EnableCheckBoxSelect="false" DataKeyNames="ID"
                            AllowSorting="true" OnSort="Grid1_Sort" SortField="pn" SortDirection="DESC" 
                        AllowPaging="true" IsDatabasePaging="true" OnPageIndexChange="Grid1_PageIndexChange" AutoScroll="true"
                        EnableMultiSelect="false" OnRowSelect="Grid1_RowSelect" EnableRowClickEvent="true" EnableRowSelectEvent="true">
                        <PageItems>
                            <f:ToolbarSeparator ID="ToolbarSeparator2" runat="server">
                            </f:ToolbarSeparator>
                            <f:ToolbarText ID="ToolbarText2" runat="server" Text="每页记录数：">
                            </f:ToolbarText>
                            <f:DropDownList ID="ddlGridPageSize1" Width="80px" AutoPostBack="true" OnSelectedIndexChanged="ddlGridPageSize1_SelectedIndexChanged" runat="server">
                                <f:ListItem Text="10" Value="10"></f:ListItem>
                                <f:ListItem Text="20" Value="20"></f:ListItem>
                                <f:ListItem Text="50" Value="50"></f:ListItem>
                                <f:ListItem Text="100" Value="100"></f:ListItem>
                            </f:DropDownList>
                        </PageItems>
                        <Columns>
                            <f:RowNumberField></f:RowNumberField>
                            <f:BoundField DataField="part_name" SortField="part_name" Width="120px" HeaderText="名称"></f:BoundField>
                            <f:BoundField DataField="pn" SortField="pn" ExpandUnusedSpace="true" HeaderText="件号"></f:BoundField>
                            <f:BoundField DataField="sn" Width="80px" HeaderText="序号"></f:BoundField>
                            <f:BoundField DataField="condition" Width="80px" HeaderText="状态"></f:BoundField>
                            <f:BoundField DataField="onhand" Width="80px" HeaderText="库存"></f:BoundField>
                        </Columns>
                    </f:Grid>
                </Items>
        </f:Region>
            
            <f:Region runat="server" ID="panelRightRegion" Position="Right" Split="true" AutoScroll="true" Layout="HBox" >

                <Items>
                    <f:TabStrip  ID="TabStrip1" runat="server" AutoScroll="true" ShowBorder="True" 
                        EnableTabCloseMenu="false" ActiveTabIndex="0" Width="800px">
                        <Toolbars>
                            <f:Toolbar runat="server">
                                <Items>
                                    <f:Button ID="Button1" Icon="DatabaseSave" runat="server" Text="保存数据" OnClick="btnUpdateSelected_Click" >
                                    </f:Button>
                                </Items>
                            </f:Toolbar>
                        </Toolbars>
                            
                        <Tabs >
                            <f:Tab ID="Tab1" runat="server" Title="航材信息" Icon="Page"
                                BodyPadding="2px 5px" >
                                <Items>
                                    <f:Form ID="SimpleForm1" ShowBorder="false" ShowHeader="false" runat="server"
                                        BodyPadding="10px"  Title="SimpleForm">
                                        <Rows>
                                            <f:FormRow ID="FormRow2"  runat="server">
                                                <Items>
                                                    <f:TextBox ID="tbx_part_name" runat="server" Label="名称" Required="true" ShowRedStar="true" Readonly="true">
                                                    </f:TextBox>
                                                    <f:TextBox ID="tbx_pn" Label="件号" runat="server" Required="true" ShowRedStar="true" Readonly="true">
                                                    </f:TextBox>
                                                    <f:TextBox ID="tbx_sn" Label="序号" runat="server" Readonly="true">
                                                    </f:TextBox>
                                                     <f:TextBox ID="tbx_part_type" Label="类型" runat="server" Readonly="true">
                                                    </f:TextBox>
                                                    <f:TextBox ID="tbx_condition" Label="状态" runat="server" Readonly="true">
                                                    </f:TextBox>
                                                    <f:TextBox ID="tbx_onhand" Label="库存" runat="server" Readonly="true">
                                                    </f:TextBox>
                                                     <f:TextBox ID="tbx_loc" Label="定位" runat="server" Readonly="true">
                                                    </f:TextBox>
                                                     <f:TextBox ID="tbx_des" Label="描述" runat="server" Readonly="true">
                                                    </f:TextBox>
                                                    <f:TextArea ID="tbx_remark" Label="备注" runat="server" Readonly="true">
                                                    </f:TextArea>
                                                     <f:TextBox ID="tbx_rec_date" Label="入库日期" runat="server" Readonly="true">
                                                    </f:TextBox>
                                                </Items>
                                            </f:FormRow>

                                        </Rows>
                                    </f:Form>

                                </Items>

                            </f:Tab>
                        </Tabs>
                    </f:TabStrip>
                </Items>
        </f:Region>
        </Regions>
        </f:RegionPanel>

        <f:HiddenField ID="highlightRows" runat="server">
        </f:HiddenField>
        <f:HiddenField ID="currentPageIndex" runat="server">
        </f:HiddenField>

        <f:Window ID="Window1" CloseAction="Hide" runat="server" IsModal="true" Hidden="true" Target="Top" EnableResize="true"
             EnableMaximize="true" EnableIFrame="true" IFrameUrl="about:blank" Width="650px" Height="450px" OnClose="Window1_Close">
        </f:Window>

    </form>

</body>
</html>
