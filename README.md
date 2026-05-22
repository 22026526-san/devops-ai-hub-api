# DevOps AI Hub API - Hướng dẫn cài đặt và khởi chạy

Dự án này là Backend (API) cho nền tảng DevOps AI Hub. API được xây dựng trên nền tảng **.NET** theo kiến trúc Clean Architecture, có tích hợp AI Agents và các dịch vụ lưu trữ/email.

Dưới đây là các bước chi tiết để setup môi trường và chạy dự án ở local.

## 1. Yêu cầu hệ thống (Prerequisites)
Trước khi bắt đầu, hãy đảm bảo máy của bạn đã cài đặt:
* **.NET SDK** (phiên bản tương ứng với dự án, ví dụ .NET 8.0)
* **MySQL Server** (công cụ quản lý như MySQL Workbench)
* **Git**
* IDE: Visual Studio 2022 hoặc Visual Studio Code.
* **OLLAMA**.

---

## 2. Hướng dẫn cài đặt chi tiết

### Bước 2.1: Clone dự án và Restore Packages
Mở terminal và chạy các lệnh sau:
```bash
git clone <đường-dẫn-repo-của-bạn>
cd DevOpsAiHubAPI
dotnet restore

```

### Bước 2.2: Import Database (MySQL)

Dự án đã đính kèm sẵn file dump SQL chứa cấu trúc và dữ liệu mẫu.

1. Mở hệ quản trị cơ sở dữ liệu MySQL của bạn.
2. Tạo một database mới (ví dụ: `devops_ai_hub_db`).
3. Tìm file SQL trong source code tại đường dẫn:
`DevOpsAiHub.Infrastructure/Database/dataDevOpsAiHub.sql`
4. Thực thi (Import) file `.sql` này vào database vừa tạo.

Dưới đây là phần nội dung **Bước 2.3** đã được viết lại chi tiết hơn, bao gồm cả hướng dẫn cách tải các file model (bạn có thể cung cấp link Drive cá nhân của bạn, hoặc hướng dẫn họ tải trực tiếp từ Hugging Face). Bạn chỉ cần copy đoạn này thay thế vào file README:

---

### Bước 2.3: Cài đặt AI Model cho tính năng Rerank (BGE-Reranker)

Hệ thống sử dụng model `bge-reranker-v2-m3` để tối ưu kết quả tìm kiếm (RAG pipeline). Do dung lượng các file model ONNX lớn nên đã được đưa vào `.gitignore` và không có sẵn trên Git. Bạn có thể tải bản chuẩn định dạng ONNX trực tiếp từ Hugging Face:

1. Truy cập vào trang Hugging Face có chứa model định dạng ONNX (ví dụ nhánh ONNX của các repo convert sẵn).
2. Tải thủ công đúng 4 file sau về máy:
* `model.onnx`
* `model.onnx_data`
* `sentencepiece.bpe.model`
* `tokenizer.json`



**Đưa model vào dự án:**

1. Trong source code, điều hướng đến (hoặc tạo mới) thư mục theo đúng cấu trúc sau:
`DevOpsAiHub.Infrastructure/Assets/reranker/`
2. Copy cả 4 file vừa tải vào bên trong thư mục `reranker`.

Việc tích hợp thêm Ollama cho các model Embedding và LLM nội bộ sẽ giúp hệ thống của bạn hoạt động độc lập và cực kỳ tối ưu. Các model này sẽ đóng vai trò cốt lõi cho các AI Agents xử lý RAG và truy xuất, giải đáp các kiến thức chuyên sâu một cách chính xác.

Dưới đây là phần bổ sung **Bước 2.4** để hướng dẫn người dùng cài đặt Ollama, đồng thời mình cập nhật lại **Bước 2.5** (Cấu hình `appsettings.json`) để bao gồm đoạn config bạn vừa cung cấp. Bạn có thể chèn trực tiếp vào file `README.md` của mình:

---

### Bước 2.4: Cài đặt Ollama và tải AI Models (Embedding & Chat)

Hệ thống sử dụng **Ollama** để chạy các mô hình ngôn ngữ lớn (LLM) và Embedding model ở local nhằm đảm bảo tính bảo mật và tối ưu cho quá trình truy xuất RAG. Bạn cần cài đặt Ollama và tải về các model tương ứng.

**1. Cài đặt Ollama:**

* Truy cập trang chủ [Ollama](https://ollama.com/download) và tải bản cài đặt phù hợp với hệ điều hành của bạn (Windows/macOS/Linux).
* Khởi chạy ứng dụng và đảm bảo biểu tượng Ollama đang xuất hiện ở khay hệ thống.

**2. Tải các model cần thiết:**
Mở Terminal (hoặc Command Prompt / PowerShell) và chạy lần lượt các lệnh sau:

* **Tải Embed Model (`bge-m3:567m`):**
Model này dùng để nhúng (embed) dữ liệu văn bản thành vector.
```bash
ollama pull bge-m3:567m

```


* **Tải Chat Model (`qwen2.5:7b`):**
Model ngôn ngữ chính (LLM) đóng vai trò là AI Agent tương tác với người dùng.
```bash
ollama pull qwen2.5:7b

```

À, tôi hiểu rồi! Vậy thì Docker Compose trong dự án của bạn đóng vai trò cung cấp hạ tầng Vector Database (Qdrant) phục vụ cho tính năng RAG, chứ không phải đóng gói toàn bộ ứng dụng API.

Dưới đây là nội dung được điều chỉnh lại cho chính xác. Bạn có thể chèn phần này thành **Bước 2.5** (ngay sau bước cài đặt Ollama) trong file `README.md` của Backend:

---

### Bước 2.5: Khởi chạy Qdrant bằng Docker Compose

Hệ thống sử dụng **Qdrant** làm cơ sở dữ liệu vector để lưu trữ và truy xuất các vector embeddings cho hệ thống RAG (Retrieval-Augmented Generation). Dự án đã có sẵn file `docker-compose.yml` để dựng môi trường này.

**1. Yêu cầu:** Máy tính của bạn cần được cài đặt sẵn và đang chạy [Docker Desktop]

**2. Lệnh khởi chạy:**
Mở terminal tại thư mục gốc của dự án (nơi chứa file `docker-compose.yml`) và chạy lệnh sau:

```bash
dockercompose up -d

```

**3. Kiểm tra Qdrant UI (Tùy chọn):**
Mặc định, Qdrant cung cấp một giao diện quản lý trên nền web. Sau khi container chạy thành công, bạn có thể truy cập vào đường dẫn sau trên trình duyệt để kiểm tra các vector collections:
👉 `http://localhost:6333/dashboard`


---

### Bước 2.6: Cấu hình `appsettings.json`

Bạn cần cấu hình chuỗi kết nối Database, API Keys và thông tin local server của Ollama. Mở dự án `DevOpsAiHub.Api` và cập nhật file `appsettings.Development.json` (hoặc `appsettings.json`) bằng thông tin tài khoản cá nhân của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=devops_ai_hub_db;User=root;Password=mat_khau_cua_ban;"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "EmbedModel": "bge-m3:567m",
    "ChatModel": "qwen2.5:7b"
  },
  "Cloudinary": {
    "CloudName": "your_cloud_name",
    "ApiKey": "your_api_key",
    "ApiSecret": "your_api_secret"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "DevOps AI Hub",
    "SenderEmail": "your_email@gmail.com",
    "Password": "your_app_password" 
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

```
---

## 3. Khởi chạy dự án

Sau khi đã hoàn tất các bước cấu hình:

1. Đặt `DevOpsAiHub.Api` làm **Startup Project** (Nếu dùng Visual Studio).
2. Nhấn `F5` hoặc nút **Run** (chọn profile chạy là `https` hoặc `IIS Express`).
3. Hoặc chạy qua CLI tại thư mục `DevOpsAiHub.Api`:
```bash
dotnet run

```


4. Truy cập giao diện Swagger để test API tại: `https://localhost:<port>/swagger`

---

