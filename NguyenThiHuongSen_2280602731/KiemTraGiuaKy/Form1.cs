using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace KiemTraGiuaKy
{
    public partial class frmSinhVien : Form
    {
        private bool isEditing = false;
        public frmSinhVien()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
            LoadLopHoc();
            SetButtonState(true);
            btnTimKiem.Click += btnTimKiem_Click;

        }
        private void SetButtonState(bool isInitial)
        {
            btnThem.Enabled = isInitial;
            btnXoa.Enabled = isInitial;
            btnSua.Enabled = isInitial;
            btnLuu.Enabled = !isInitial;
            btnKhongLuu.Enabled = !isInitial;// Khi ở trạng thái ban đầu, không cho phép Lưu
            btnThoat.Enabled = true; // Luôn cho phép thoát
        }
        private void LoadData()
        {
            using (var context = new StudentModel()) // Sử dụng StudentModel làm DbContext
            {
                // Lấy dữ liệu từ bảng SinhVien và Lop
                var sinhVienList = context.SinhVien
                    .Select(sv => new
                    {
                        sv.MaSV,
                        sv.HoTenSV,
                        sv.NgaySinh,
                        sv.MaLop,
                        TenLop = sv.Lop.TenLop // Lấy tên lớp từ bảng Lop
                    })
                    .ToList();

                // Gán dữ liệu cho DataGridView
                dgvSinhVien.DataSource = sinhVienList;
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void frmSinhVien_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát chương trình?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = true; // Hủy bỏ việc đóng form nếu người dùng chọn "Không"
            }
        }



        private void LoadLopHoc()
        {
            using (var context = new StudentModel())
            {
                // Lấy danh sách các lớp từ bảng Lop
                var lopHocList = context.Lop
                    .Select(l => new
                    {
                        l.MaLop,
                        l.TenLop
                    })
                    .ToList();

                // Gán dữ liệu cho ComboBox
                cmbLopHoc.DataSource = lopHocList;
                cmbLopHoc.DisplayMember = "TenLop"; // Hiển thị tên lớp
                cmbLopHoc.ValueMember = "MaLop";    // Giá trị sẽ là mã lớp
            }
        }



        private void dgvSinhVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Kiểm tra nếu dòng được chọn hợp lệ
            {
                // Lấy dòng được chọn
                DataGridViewRow selectedRow = dgvSinhVien.Rows[e.RowIndex];

                // Gán dữ liệu từ dòng được chọn vào các control
                txtMaSV.Text = selectedRow.Cells["MaSV"].Value.ToString();
                txtHoTenSV.Text = selectedRow.Cells["HoTenSV"].Value.ToString();

                // Gán ngày sinh nếu có
                if (selectedRow.Cells["NgaySinh"].Value != DBNull.Value)
                {
                    dtNgaySinh.Value = Convert.ToDateTime(selectedRow.Cells["NgaySinh"].Value);
                }
                else
                {
                    dtNgaySinh.Value = DateTime.Now; // Gán giá trị mặc định nếu không có ngày sinh
                }

                // Gán giá trị cho ComboBox lớp học
                cmbLopHoc.SelectedValue = selectedRow.Cells["MaLop"].Value.ToString();
            }
        }

        private void ClearInputFields()
        {
            txtMaSV.Clear();
            txtHoTenSV.Clear();
            dtNgaySinh.Value = DateTime.Now;
            cmbLopHoc.SelectedIndex = -1; // Nếu bạn muốn chọn không có lớp nào
        }


        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (isAdding) // Nếu đang trong trạng thái thêm mới
            {
                // Kiểm tra thông tin đầu vào
                if (string.IsNullOrWhiteSpace(txtMaSV.Text) || string.IsNullOrWhiteSpace(txtHoTenSV.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using (var context = new StudentModel())
                    {
                        // Kiểm tra xem mã sinh viên đã tồn tại chưa
                        var existingStudent = context.SinhVien.Find(txtMaSV.Text);
                        if (existingStudent != null)
                        {
                            MessageBox.Show("Mã sinh viên đã tồn tại. Vui lòng nhập mã khác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Tạo sinh viên mới
                        var newStudent = new SinhVien
                        {
                            MaSV = txtMaSV.Text,
                            HoTenSV = txtHoTenSV.Text,
                            NgaySinh = dtNgaySinh.Value,
                            MaLop = cmbLopHoc.SelectedValue.ToString()
                        };

                        // Thêm vào cơ sở dữ liệu
                        context.SinhVien.Add(newStudent);
                        context.SaveChanges();
                    }

                    // Thông báo lưu thành công
                    MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadData(); // Tải lại danh sách sinh viên lên DataGridView
                    ClearInputFields(); // Xóa các trường nhập liệu
                    SetButtonState(true); // Đặt trạng thái ban đầu cho các nút
                    isAdding = false; // Tắt trạng thái thêm mới
                    txtMaSV.Enabled = false; // Vô hiệu hóa trường mã sinh viên
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thêm sinh viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (isDeleting) // Kiểm tra nếu đang trong quá trình xóa
            {
                using (var context = new StudentModel())
                {
                    var studentToDelete = context.SinhVien.Find(txtMaSV.Text);

                    if (studentToDelete != null)
                    {
                        context.SinhVien.Remove(studentToDelete);
                        context.SaveChanges(); // Thực hiện xóa sinh viên khỏi cơ sở dữ liệu

                        MessageBox.Show("Xóa sinh viên thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Cập nhật lại DataGridView và xóa các trường nhập
                        LoadData();
                        ClearInputFields();
                    }
                }

                isDeleting = false; // Đặt lại trạng thái
                SetButtonState(true); // Trở lại trạng thái ban đầu
            }
            if (isEditingg) // Kiểm tra nếu đang trong quá trình sửa
            {
                using (var context = new StudentModel())
                {
                    var studentToEdit = context.SinhVien.Find(txtMaSV.Text);

                    if (studentToEdit != null)
                    {
                        // Cập nhật thông tin sinh viên
                        studentToEdit.HoTenSV = txtHoTenSV.Text;
                        studentToEdit.NgaySinh = dtNgaySinh.Value;
                        studentToEdit.MaLop = cmbLopHoc.SelectedValue.ToString();

                        context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                        MessageBox.Show("Cập nhật sinh viên thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Cập nhật lại DataGridView
                        LoadData();
                        ClearInputFields();
                    }
                }

                isEditingg = false; // Đặt lại trạng thái
                SetButtonState(true); // Quay lại trạng thái ban đầu
            }
        }
        private bool isAdding = false;

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAdding = true; // Kích hoạt trạng thái thêm mới
            ClearInputFields(); // Xóa các trường nhập liệu
            SetButtonState(false); // Đổi trạng thái các nút
            txtMaSV.Enabled = true; // Cho phép nhập mã sinh viên
        }

        private void btnKhongLuu_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn không lưu thao tác này saoo?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ClearInputFields();
                SetButtonState(true);
                isAdding = false;
                txtMaSV.Enabled = false;
            }
            if (isEditingg) // Nếu đang trong quá trình sửa
            {
                if (result == DialogResult.Yes)
                {
                    // Hủy bỏ việc sửa và quay lại trạng thái ban đầu
                    isEditing = false;
                    ClearInputFields(); // Xóa các trường nhập
                    LoadData(); // Cập nhật lại dữ liệu
                    SetButtonState(true); // Quay lại trạng thái nút ban đầu
                }
                else
                {
                    MessageBox.Show("Tiếp tục chỉnh sửa sinh viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private bool isDeleting = false;
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaSV.Text))
            {
                MessageBox.Show("Vui lòng chọn sinh viên để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmDelete = MessageBox.Show("Bạn có chắc chắn muốn xóa sinh viên này không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmDelete == DialogResult.Yes)
            {
                isDeleting = true; // Đánh dấu đang thực hiện thao tác xóa
                SetButtonState(false); // Chuyển sang trạng thái sẵn sàng lưu hoặc không lưu
                MessageBox.Show("Nhấn Lưu để xác nhận việc xóa, hoặc Không Lưu để hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool isEditingg = false;
        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaSV.Text))
            {
                MessageBox.Show("Vui lòng chọn sinh viên để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Đánh dấu đang sửa
            isEditingg = true;
            SetButtonState(false); // Chuyển sang trạng thái cho phép lưu hoặc không lưu
            MessageBox.Show("Chỉnh sửa thông tin và nhấn Lưu để xác nhận, hoặc Không Lưu để hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

      
        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            string searchName = txtTimKiem.Text.Trim(); // Lấy tên sinh viên từ TextBox

            using (var context = new StudentModel())
            {
                // Tìm kiếm sinh viên theo tên
                var sinhVienList = context.SinhVien
                    .Where(sv => sv.HoTenSV.Contains(searchName)) // Tìm theo tên sinh viên
                    .Select(sv => new
                    {
                        sv.MaSV,
                        sv.HoTenSV,
                        sv.NgaySinh,
                        sv.MaLop,
                        TenLop = sv.Lop.TenLop // Lấy tên lớp từ bảng Lop
                    })
                    .ToList();

                // Gán dữ liệu cho DataGridView
                dgvSinhVien.DataSource = sinhVienList;

                // Kiểm tra có kết quả tìm kiếm hay không
                if (sinhVienList.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy sinh viên nào với tên '" + searchName + "'.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
