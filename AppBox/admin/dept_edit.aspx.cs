using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using System.Data.Entity;
using FineUI;

namespace AppBox.admin
{
    public partial class dept_edit : PageBase
    {
        #region ViewPower

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public override string ViewPower
        {
            get
            {
                return "CoreDeptEdit";
            }
        }

        #endregion

        #region Page_Load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            btnClose.OnClientClick = ActiveWindow.GetHideReference();

            int id = GetQueryIntValue("id");
            Dept current = DB.Depts.Include(d => d.Parent)
                .Where(d => d.ID == id).FirstOrDefault();
            if (current == null)
            {
                // 参数错误，首先弹出Alert对话框然后关闭弹出窗口
                Alert.Show("参数错误！", String.Empty, ActiveWindow.GetHideReference());
                return;
            }

            tbxName.Text = current.Name;
            tbxSortIndex.Text = current.SortIndex.ToString();
            tbxRemark.Text = current.Remark;

            // 绑定下拉列表
            BindDDL(current);
        }

        private void BindDDL(Dept current)
        {
            List<Dept> mys = ResolveDDL<Dept>(DeptHelper.Depts, current.ID);

            // 绑定到下拉列表（启用模拟树功能和不可选择项功能）
            ddlParent.EnableSimulateTree = true;
            ddlParent.DataTextField = "Name";
            ddlParent.DataValueField = "ID";
            ddlParent.DataSimulateTreeLevelField = "TreeLevel";
            ddlParent.DataEnableSelectField = "Enabled";
            ddlParent.DataSource = mys;
            ddlParent.DataBind();

            if (current.Parent != null)
            {
                // 选中当前节点的父节点
                ddlParent.SelectedValue = current.Parent.ID.ToString();
            }
        }


        #endregion

        #region Events

        protected void btnSaveClose_Click(object sender, EventArgs e)
        {
            int id = GetQueryIntValue("id");
            Dept item = DB.Depts.Include(d => d.Parent).Where(d => d.ID == id).FirstOrDefault();
            item.Name = tbxName.Text.Trim();
            item.SortIndex = Convert.ToInt32(tbxSortIndex.Text.Trim());
            item.Remark = tbxRemark.Text.Trim();

            int parentID = Convert.ToInt32(ddlParent.SelectedValue);
            if (parentID == -1)
            {
                item.Parent = null;
            }
            else
            {
                Dept dept = Attach<Dept>(parentID);
                item.Parent = dept;
            }

            DB.SaveChanges();

            //FineUI.Alert.Show("保存成功！", String.Empty, FineUI.Alert.DefaultIcon, FineUI.ActiveWindow.GetHidePostBackReference());
            PageContext.RegisterStartupScript(ActiveWindow.GetHidePostBackReference());
        }

        #endregion

    }
}
