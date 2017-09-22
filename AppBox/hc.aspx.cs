using FineUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;
using EntityFramework.Extensions;
using System.Data;

namespace AppBox
{
    public partial class hc : PageBase
    {
        #region ViewPower

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public override string ViewPower
        {
            get
            {
                return "";
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
            Grid1.PageSize = ConfigHelper.PageSize;
            ddlGridPageSize1.SelectedValue = ConfigHelper.PageSize.ToString();

            BindGrid1();
        }


        private void BindGrid1()
        {
            IQueryable<Hc> q = DB.Hcs;
            // 在用户名称中搜索
            string searchText = ttbSearchRo.Text.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.part_name.Contains(searchText) || u.pn.Contains(searchText));
            }
            // 在查询添加之后，排序和分页之前获取总记录数
            Grid1.RecordCount = q.Count();

            // 排列和分页
            q = SortAndPage<Hc>(q, Grid1);
            Grid1.DataSource = q;
            Grid1.DataBind();
        }

        private void GetHcDetails(Hc h)
        {
            tbx_part_name.Text = h.part_name;
            tbx_pn.Text = h.pn;
            tbx_sn.Text = h.sn;
            DropDownList_Part_Type.SelectedValue = h.part_type;
            DropDownList_Condition.SelectedValue = h.condition;
            tbx_loc.Text = h.loc;
            tbx_onhand.Text = h.onhand.ToString();
            tbx_des.Text = h.part_des;
            tbx_remark.Text = h.remark;
            tbx_rec_date.SelectedDate = h.rec_date;
        }

        #endregion


        #region Grid1 Events

        protected void Grid1_Sort(object sender, GridSortEventArgs e)
        {
            Grid1.SortDirection = e.SortDirection;
            Grid1.SortField = e.SortField;
            BindGrid1();
        }

        protected void Grid1_RowSelect(object sender, FineUI.GridRowSelectEventArgs e)
        {
            int roleID = GetSelectedDataKeyID(Grid1);

            if (roleID == -1)
            {
            }
            else
            {
                Hc hc = DB.Hcs.Find(roleID);
                GetHcDetails(hc);
            }

        }
        protected void Grid1_PageIndexChange(object sender, GridPageEventArgs e)
        {
            Grid1.PageIndex = e.NewPageIndex;
            BindGrid1();
        }
        protected void ddlGridPageSize1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Grid1.PageSize = Convert.ToInt32(ddlGridPageSize1.SelectedValue);

            BindGrid1();
        }
        protected void ttbSearchRo_Trigger2Click(object sender, EventArgs e)
        {
            ttbSearchRo.ShowTrigger1 = true;
            BindGrid1();
        }

        protected void ttbSearchRo_Trigger1Click(object sender, EventArgs e)
        {
            ttbSearchRo.Text = String.Empty;
            ttbSearchRo.ShowTrigger1 = false;
            BindGrid1();
        }

        #endregion

        protected void btnUpdateSelected_Click(object sender, EventArgs e)
        {
            // 在操作之前进行权限检查
            if (!CheckPower("HcView"))
            {
                CheckPowerFailWithAlert();
                return;
            }
            int ID = GetSelectedDataKeyID(Grid1);
            if (ID != -1)
            {
                //保存工卡信息
                Hc h = DB.Hcs.Where(u => u.ID == ID).FirstOrDefault();
                h.part_name = tbx_part_name.Text;
                h.pn = tbx_pn.Text;
                h.sn = tbx_sn.Text;
                h.part_type = DropDownList_Part_Type.SelectedValue;
                h.condition = DropDownList_Condition.SelectedValue;
                h.loc = tbx_loc.Text;
                h.onhand = Convert.ToInt32(tbx_onhand.Text);
                h.part_des = tbx_des.Text;
                h.remark = tbx_remark.Text;
                h.rec_date = Convert.ToDateTime(tbx_rec_date.Text);

                DB.SaveChanges();

                Alert.Show("航材数据更新成功！");
                SimpleForm1.Reset();
                BindGrid1();
            }
            else
            {
                Alert.Show("请选择要更新的航材！");
            }
        }
        //更新代码
        protected void Window1_Close(object sender, EventArgs e)
        {
            BindGrid1();
        }

        protected void Grid1_RowCommand(object sender, GridCommandEventArgs e)
        {
            object[] values = Grid1.DataKeys[e.RowIndex];
            int HcID = Convert.ToInt32(values[0]);

            if (e.CommandName == "Delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("HcView"))
                {
                    CheckPowerFailWithAlert();
                    return;
                }

                //删除指定工卡
                DB.Hcs.Where(d => d.ID == HcID).Delete();

                BindGrid1();

            }
        }
    }
}