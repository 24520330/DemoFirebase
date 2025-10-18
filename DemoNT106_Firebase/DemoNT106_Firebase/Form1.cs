using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FireSharp.Config;//Thư viện chứa các lớp cấu hình để kết nối đến Firebase
using FireSharp.Response;//Thư viện cung cấp các lớp để nhận và xử lí phản hồi từ Firebase
using FireSharp.Interfaces;/*Thư viện định nghĩa interface như IFirebaseClient cho phép
                           tạo đối tượng client để thao tác với dữ liệu trên Firebase*/

namespace DemoNT106_Firebase
{
    public partial class Form1 : Form
    {
        //Thiết lập Firebase: cung cấp "Khóa bí mật (AuthSecret)" và "Đường dẫn (BasePath)"
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "4gxx3lB4spREB50aV2Vo0w6dgXTaDugSo9Bk9hxw",
            //Mọi người có thể sao chép đường dẫn và truy cập vào Realtime Firebase
            BasePath = "https://demont106-c9287-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        
        //Đối tượng client dùng để giao tiếp với Firebase
        IFirebaseClient client;
       
        public Form1()
        {
            InitializeComponent();
        }
        
        //Tiến hành kết nối với Firebase ngay khi form Demo được tải lên
        private void Form1_Load(object sender, EventArgs e)
        {
            client = new FireSharp.FirebaseClient(config);
                if (client != null )
            {
                lbStatus.Text = "Đã kết nối";
            }
        }
        
        //Nút "Thêm": thêm dữ liệu mới trực tiếp vào Firebase
        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var datalayer = new Data
            {
                ID = txtID.Text,
                Name = txtName.Text,
                Gender = txtGender.Text
            };
            //Gửi dữ liệu lên Firebase, lưu tại nhánh "Information/<ID đã nhập vào>/"
            SetResponse resp = await client.SetTaskAsync("Information/"+ txtID.Text, datalayer);
            //Trả về kết quả
            Data result = resp.ResultAs<Data>();
            //Thông báo dữ liệu đã được thêm vào Firebase thành công
            MessageBox.Show("Dữ liệu " + result.ID + " đã được thêm vào!");
        }
        //Nút "Xóa": chỉ xóa toàn bộ nội dung trong các text box, không ảnh hưởng tới Firebase
        private void btnDelete_Click(object sender, EventArgs e)
        {
            txtID.Clear();
            txtName.Clear();
            txtGender.Clear();
        }

        /*Nút "Chỉnh sửa: cập nhập dữ liệu mới cho dữ liệu đang có trong Firebase, nhận diện
       sự tồn tại của dữ liệu thông qua kiểm tra ID*/
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Vui lòng nhập ID để chỉnh sửa!");
                return;
            }
            // Kiểm tra xem dữ liệu có tồn tại không
            FirebaseResponse check = await client.GetTaskAsync("Information/" + txtID.Text);
            if (check.Body == "null")
            {
                MessageBox.Show("Không tìm thấy ID này trong cơ sở dữ liệu!");
                return;
            }
            // Dữ liệu mới sau khi chỉnh sửa
            var updateData = new Data
            {
                ID = txtID.Text,
                Name = txtName.Text,
                Gender = txtGender.Text
            };
            // Cập nhật dữ liệu
            FirebaseResponse resp = await client.UpdateTaskAsync("Information/" + txtID.Text, updateData);
            Data result = resp.ResultAs<Data>();

            MessageBox.Show("Thông tin của ID " + result.ID + " đã được cập nhật thành công!");
        }

        /*Nút "Xóa dữ liệu": Xóa dữ liệu đang tồn tại trong Firebase, kiểm tra sự tồn tại 
         của dữ liệu thông qua ID*/
        private async void btnDataDelete_Click(object sender, EventArgs e)
        {
            //Kiểm tra xem đã nhập ID dữ liệu muốn xóa hay chưa
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Vui lòng nhập ID cần xóa!");
                return;
            }
            // Xác nhận trước khi xóa
            DialogResult confirm = MessageBox.Show(
                "Bạn có chắc muốn xóa dữ liệu với ID: " + txtID.Text + " không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (confirm == DialogResult.No)
                return;
            // Kiểm tra xem ID có tồn tại trên Firebase không
            FirebaseResponse check = await client.GetTaskAsync("Information/" + txtID.Text);
            if (check.Body == "null")
            {
                MessageBox.Show("Không tìm thấy ID này trong cơ sở dữ liệu!");
                return;
            }
            // Thực hiện xóa dữ liệu khỏi Firebase
            FirebaseResponse resp = await client.DeleteTaskAsync("Information/" + txtID.Text);
            MessageBox.Show("Dữ liệu với ID " + txtID.Text + " đã được xóa khỏi Firebase!");
            // Xóa luôn nội dung trong các text box
            txtID.Clear();
            txtName.Clear();
            txtGender.Clear();
        }
    }
}
