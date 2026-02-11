# Manruka System - Backend API ⚙️

**Manruka API** adalah layanan RESTful API yang dibangun menggunakan **.NET 8** untuk menangani logika bisnis sistem peminjaman ruangan kampus. API ini menyediakan autentikasi, manajemen ruangan, validasi jadwal (conflict detection), dan manajemen persetujuan (approval).

---

## 📋 Fitur Utama Backend

1.  **Authentication & Authorization:**
    * Login & Register.
    * Role-based access (User vs Admin).
2.  **Room Booking Logic:**
    * **Create:** Mencegah booking ganda di jam yang sama (*Conflict Detection*).
    * **Edit:** Memungkinkan revisi jadwal selama status masih *Pending*.
    * **Delete:** Pembatalan pengajuan oleh user.
3.  **Approval Workflow:**
    * Endpoint khusus Admin untuk `Approve` atau `Reject`.
4.  **Data Seeding:**
    * Otomatis mengisi database dengan data awal (Akun Admin, Daftar Ruangan, Prodi) saat aplikasi pertama kali dijalankan.

---

## 🛠 Prasyarat Sistem

Pastikan di komputer Anda sudah terinstal:
* [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* Database Server (PostgreSQL atau SQL Server, sesuai `appsettings.json`).
* Postman / Swagger (untuk testing API).

---

## 🚀 Instalasi & Menjalankan Server

Ikuti langkah ini untuk menjalankan backend di local machine:

### 1. Clone Repositori
```bash
git clone https://github.com/navyzuu/2026-manruka-backend.git
cd manruka-backend
```

### 2. Konfigurasi Database (appsettings.json)
Buka file appsettings.json dan sesuaikan ConnectionStrings dengan kredensial database lokal Anda.

Contoh (jika menggunakan PostgreSQL):
```bash
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=ManrukaDB;Username=postgres;Password=password_anda"
}
```

### 3. Migrasi Database & Seeding
Jalankan perintah berikut untuk membuat database dan mengisi data awal (Admin & Ruangan):
```bash
dotnet ef database update
```
Jika error, pastikan Anda sudah menginstal tool EF Core: 
```bash
dotnet tool install --global dotnet-ef
```

### 4. Jalankan Server
```bash
dotnet run
```

Jika berhasil, terminal akan menampilkan port yang aktif, misalnya:
```bash 
Now listening on: http://localhost:5000
```
---
## ⚠️ Catatan untuk Frontend Developer
Agar Frontend (React/Vite) bisa terhubung ke Backend ini, pastikan hal berikut:
1.  **CORS Policy:**
    * Pastikan Program.cs sudah mengizinkan origin frontend (default Vite: http://localhost:5173).
    ```bash
    builder.Services.AddCors(options => {
    options.AddPolicy("AllowReactApp", builder => {
        builder.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader();
      });
    });
    ```

2.  **Base URL:**
    * Frontend harus menembak ke http://localhost:5000/api (atau port lain sesuai output dotnet run).
