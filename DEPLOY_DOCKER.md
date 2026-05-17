# HƯỚNG DẪN DEPLOY TOÀN BỘ 3 DỰ ÁN (CUSTOMER, ADMIN, HANDLER) LÊN CLOUD DÙNG DOCKER

Tôi đã thu dọn toàn bộ các Dockerfile vào một thư mục riêng là **`docker/`** tại gốc dự án để cấu trúc thư mục của bạn luôn gọn gàng và dễ quản lý:
* **`docker/Dockerfile.customer`**: Đóng gói cổng Customer (Người dân).
* **`docker/Dockerfile.admin`**: Đóng gói cổng Admin (Quản trị viên).
* **`docker/Dockerfile.handler`**: Đóng gói cổng Handler (Cán bộ xử lý).

Dưới đây là cách thiết lập chi tiết để đưa cả 3 lên **Render** hoặc **Railway** với cấu trúc thư mục mới:

---

## 1. PHƯƠNG ÁN A: TRIỂN KHAI TRÊN RENDER.COM (Hoàn toàn miễn phí)

Trên Render, bạn sẽ tạo **3 dịch vụ Web độc lập (Web Service)** cùng kết nối chung đến 1 repository GitHub của bạn:

### Bước 1: Deploy cổng Customer
1. Truy cập [Render.com](https://render.com/) -> Chọn **"New +"** -> **"Web Service"**.
2. Kết nối repo GitHub của bạn.
3. Cấu hình dịch vụ:
   * **Name:** `hueresolve-customer`
   * **Region:** Chọn `Singapore (Southeast Asia)` để tải nhanh nhất.
   * **Runtime:** Chọn **Docker**.
   * **Instance Type:** Chọn **Free** (Miễn phí).
4. Nhấn nút **"Advanced"** ở cuối trang:
   * Tìm mục **"Dockerfile Path"** -> Điền vào: `docker/Dockerfile.customer` (Thay vì `Dockerfile` mặc định).
5. Nhấn **"Create Web Service"**.

### Bước 2: Deploy cổng Admin
1. Chọn tiếp **"New +"** -> **"Web Service"** -> Chọn đúng repo GitHub đó.
2. Cấu hình dịch vụ:
   * **Name:** `hueresolve-admin`
   * **Region:** `Singapore`
   * **Runtime:** **Docker**
3. Nhấp chọn **"Advanced"**:
   * Tại mục **"Dockerfile Path"** -> Điền vào: `docker/Dockerfile.admin`
4. Nhấn **"Create Web Service"**.

### Bước 3: Deploy cổng Handler
1. Chọn tiếp **"New +"** -> **"Web Service"** -> Chọn đúng repo GitHub đó.
2. Cấu hình dịch vụ:
   * **Name:** `hueresolve-handler`
   * **Region:** `Singapore`
   * **Runtime:** **Docker**
3. Nhấp chọn **"Advanced"**:
   * Tại mục **"Dockerfile Path"** -> Điền vào: `docker/Dockerfile.handler`
4. Nhấn **"Create Web Service"**.

**Kết quả trên Render:** Bạn sẽ có 3 link web riêng biệt, ví dụ:
* `https://hueresolve-customer.onrender.com`
* `https://hueresolve-admin.onrender.com`
* `https://hueresolve-handler.onrender.com`

---

## 2. PHƯƠNG ÁN B: TRIỂN KHAI TRÊN RAILWAY.APP (Giao diện trực quan)

Railway hỗ trợ quản lý dự án kiểu Monorepo (Nhiều ứng dụng chung 1 Repo) cực kỳ mượt mà.

### Các bước thực hiện:
1. Đăng nhập [Railway.app](https://railway.app/) -> Tạo Project mới -> Chọn **"Deploy from GitHub repo"** và chọn repository dự án của bạn.
2. Mặc định Railway sẽ build ứng dụng đầu tiên. Hãy đổi tên dịch vụ này thành `Customer` trong phần **Settings**:
   * Vào **Settings** -> **Build** -> Tại mục **Dockerfile Path** điền: `docker/Dockerfile.customer`.
3. Để thêm cổng **Admin**:
   * Trên màn hình sơ đồ dự án, nhấn nút **"New"** -> Chọn **"Github Repo"** -> Chọn lại chính repository đó.
   * Hệ thống sẽ tạo ra dịch vụ thứ 2. Vào **Settings** của dịch vụ này -> Đổi tên thành `Admin`.
   * Tại mục **Build** -> **Dockerfile Path** -> Điền vào: `docker/Dockerfile.admin`.
4. Để thêm cổng **Handler**:
   * Nhấn tiếp nút **"New"** -> Chọn **"Github Repo"** -> Chọn lại repository đó.
   * Vào **Settings** của dịch vụ thứ 3 -> Đổi tên thành `Handler`.
   * Tại mục **Build** -> **Dockerfile Path** -> Điền vào: `docker/Dockerfile.handler`.

**Kết quả trên Railway:** Cả 3 web app sẽ chạy song song trên cùng một giao diện điều khiển của Railway, rất dễ dàng quản lý!

---

> [!IMPORTANT]
> **LƯU Ý QUAN TRỌNG VỀ ĐƯỜNG LINK LIÊN KẾT GIỮA CÁC CỔNG:**
> Trong code của dự án, nếu có những chỗ điều hướng cứng (Redirect) hoặc liên kết URL giữa 3 cổng (ví dụ: Trang Admin có nút bấm link sang trang Customer hoặc ngược lại), bạn cần đảm bảo rằng các đường dẫn đó cấu hình bằng cấu hình tương đối hoặc được cập nhật bằng URL mới sau khi deploy lên Cloud nhé!
