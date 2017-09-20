using System;
using System.Data.Entity;
using System.Linq;
using FineUI;

namespace AppBox.admin
{
    public partial class user_edit : PageBase
    {
        #region ViewPower

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public override string ViewPower
        {
            get
            {
                return "CoreUserEdit";
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
            User current = DB.Users
                .Include(u => u.Dept)
                .Include(u => u.Roles)
                .Include(u => u.Titles)
                .Where(u => u.ID == id).FirstOrDefault();
            if (current == null)
            {
                // 参数错误，首先弹出Alert对话框然后关闭弹出窗口
                Alert.Show("参数错误！", String.Empty, ActiveWindow.GetHideReference());
                return;
            }

            if (current.Name == "admin" && GetIdentityName() != "admin")
            {
                Alert.Show("你无权编辑超级管理员！", String.Empty, ActiveWindow.GetHideReference());
                return;
            }

            labName.Text = current.Name;
            tbxRealName.Text = current.ChineseName;
            tbxCompanyEmail.Text = current.CompanyEmail;
            tbxEmail.Text = current.Email;
            tbxCellPhone.Text = current.CellPhone;
            tbxOfficePhone.Text = current.OfficePhone;
            tbxOfficePhoneExt.Text = current.OfficePhoneExt;
            tbxHomePhone.Text = current.HomePhone;
            tbxRemark.Text = current.Remark;
            cbxEnabled.Checked = current.Enabled;
            ddlGender.SelectedValue = current.Gender;

            // 初始化用户所属角色
            InitUserRole(current);

            // 初始化用户所属部门
            InitUserDept(current);

            // 初始化用户所属职称
            InitUserTitle(current);
        }

        #region InitUserRole

        private void InitUserDept(User current)
        {
            if (current.Dept != null)
            {
                tbSelectedDept.Text = current.Dept.Name;
                hfSelectedDept.Text = current.Dept.ID.ToString();
            }

            // 打开编辑窗口
            string selectDeptURL = String.Format("./user_select_dept.aspx?ids=<script>{0}</script>", hfSelectedDept.GetValueReference());
            tbSelectedDept.OnClientTriggerClick = Window1.GetSaveStateReference(hfSelectedDept.ClientID, tbSelectedDept.ClientID)
                    + Window1.GetShowReference(selectDeptURL, "选择用户所属的部门");

        }

        #endregion

        #region InitUserRole

        private void InitUserRole(User current)
        {
            tbSelectedRole.Text = String.Join(",", current.Roles.Select(u => u.Name).ToArray());
            hfSelectedRole.Text = String.Join(",", current.Roles.Select(u => u.ID).ToArray());

            // 打开编辑角色的窗口
            string selectRoleURL = String.Format("./user_select_role.aspx?ids=<script>{0}</script>", hfSelectedRole.GetValueReference());
            tbSelectedRole.OnClientTriggerClick = Window1.GetSaveStateReference(hfSelectedRole.ClientID, tbSelectedRole.ClientID)
                    + Window1.GetShowReference(selectRoleURL, "选择用户所属的角色");

        }
        #endregion

        #region InitUserTitle

        private void InitUserTitle(User current)
        {
            tbSelectedTitle.Text = String.Join(",", current.Titles.Select(u => u.Name).ToArray()); ;
            hfSelectedTitle.Text = String.Join(",", current.Titles.Select(u => u.ID).ToArray()); ;

            // 打开编辑角色的窗口
            string selectTitleURL = String.Format("./user_select_title.aspx?ids=<script>{0}</script>", hfSelectedTitle.GetValueReference());
            tbSelectedTitle.OnClientTriggerClick = Window1.GetSaveStateReference(hfSelectedTitle.ClientID, tbSelectedTitle.ClientID)
                    + Window1.GetShowReference(selectTitleURL, "选择用户拥有的职称");

        }
        #endregion


        #endregion

        #region Events

        protected void btnSaveClose_Click(object sender, EventArgs e)
        {
            int id = GetQueryIntValue("id");
            User item = DB.Users
                .Include(u => u.Dept)
                .Include(u => u.Roles)
                .Include(u => u.Titles)
                .Where(u => u.ID == id).FirstOrDefault();
            //item.Name = tbxName.Text.Trim();
            item.ChineseName = tbxRealName.Text.Trim();
            item.Gender = ddlGender.SelectedValue;
            item.CompanyEmail = tbxCompanyEmail.Text.Trim();
            item.Email = tbxEmail.Text.Trim();
            item.CellPhone = tbxCellPhone.Text.Trim();
            item.OfficePhone = tbxOfficePhone.Text.Trim();
            item.OfficePhoneExt = tbxOfficePhoneExt.Text.Trim();
            item.HomePhone = tbxHomePhone.Text.Trim();
            item.Remark = tbxRemark.Text.Trim();
            item.Enabled = cbxEnabled.Checked;


            if (String.IsNullOrEmpty(hfSelectedDept.Text))
            {
                item.Dept = null;
            }
            else
            {
                int newDeptID = Convert.ToInt32(hfSelectedDept.Text);

                Dept dept = Attach<Dept>(newDeptID);
                item.Dept = dept;

            }


            int[] roleIDs = StringUtil.GetIntArrayFromString(hfSelectedRole.Text);
            ReplaceEntities<Role>(item.Roles, roleIDs);

            int[] titleIDs = StringUtil.GetIntArrayFromString(hfSelectedTitle.Text);
            ReplaceEntities<Title>(item.Titles, titleIDs);

            DB.SaveChanges();


            PageContext.RegisterStartupScript(ActiveWindow.GetHidePostBackReference());
        }

        #endregion

    }
}
