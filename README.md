# Dual Wings: Collaborative Multiplayer Space Shooter

[![Unity Version](https://img.shields.io/badge/Unity-6000.4.0f1-black.svg?style=flat-square&logo=unity)](https://unity.com/)
[![Netcode](https://img.shields.io/badge/Netcode-NGO-orange.svg?style=flat-square)](https://unity.com/products/netcode)
[![Language](https://img.shields.io/badge/C%23-.NET-blue.svg?style=flat-square)](https://learn.microsoft.com/en-us/dotnet/csharp/)

**Dual Wings** là một dự án game bắn máy bay Multiplayer (Co-op) độc đáo, nơi **hai người chơi cùng hợp tác để điều hành duy nhất một phi thuyền** với nhiệm vụ đánh bại Boss. Dự án tập trung vào việc giải quyết các thách thức về đồng bộ hóa đầu vào (Input Synchronization), tối ưu hóa tài nguyên mạng và xây dựng hệ thống Boss AI đa trạng thái.

---

## Tính năng nổi bật (Technical Highlights)

### 1. Cơ chế "Shared Control" (Collaborative Input)
* **Collaborative Steering:** Hai người chơi (Host & Client) cùng chia sẻ quyền điều khiển một thực thể (Player Plane). Hệ thống xử lý xung đột đầu vào (Input Conflict Handling) đảm bảo phi thuyền di chuyển mượt mà dựa trên sự phối hợp của cả hai bên.
* **Co-op Strategy:** Yêu cầu sự giao tiếp và phối hợp chính xác để né tránh chướng ngại vật và tối ưu hóa góc bắn trong môi trường chiến đấu tốc độ cao.

### 2. Kiến trúc Network Authoritative
* **Server-Authoritative Logic:** Mọi tính toán về va chạm (Collision), sát thương (Damage Calculation) và vị trí đều được xử lý tại Server nhằm ngăn chặn gian lận và đảm bảo tính nhất quán dữ liệu (Data Consistency).
* **State Synchronization:** Áp dụng `NetworkVariable` để đồng bộ thời gian thực các chỉ số quan trọng như trạng thái Khiên (Shield), Máu (Health) và các chỉ số nhân vật (Damage/Scale Buffs).

### 3. Tối ưu hóa hiệu suất (Optimization)
* **Network Object Pooling:** Triển khai hệ thống Pool cho hàng nghìn viên đạn, giúp giảm thiểu chi phí bộ nhớ cho việc khởi tạo/hủy (Instantiate/Destroy) liên tục, loại bỏ hoàn toàn tình trạng giật lag (Lag Spikes) trong các giai đoạn bắn đạn dày đặc.

### 4. Boss AI đa giai đoạn (Multi-phase Boss Logic)
* **Phase-based AI:** Boss thay đổi hành vi tấn công linh hoạt dựa trên lượng máu còn lại (Laser Phase, Teleport, Random Movement Patterns), mang lại trải nghiệm thử thách và chiều sâu cho Gameplay.

---

## Tech Stack

* **Engine:** Unity 6 (6000.4.0f1) - Tận dụng các tính năng mới nhất của Unity 6.
* **Networking:** Unity Netcode for GameObjects (NGO).
* **Transport:** Unity Transport (UTP) - Giao thức UDP cho độ trễ thấp.
* **UI System:** TextMeshPro & Unity UI Toolkit.
* **Tools:** Git LFS (quản lý file lớn), C# (.NET).

---

## Giải pháp Kỹ thuật & Bài học kinh nghiệm

* **Teleportation Smoothing:** Giải quyết lỗi nội suy (Interpolation) của `NetworkTransform` bằng cách triển khai phương thức `.Teleport()` thủ công khi Boss dịch chuyển tức thời, ngăn chặn hiện tượng "lướt hình" (gliding) trên máy Client.
* **Remote Procedure Calls (RPCs):** Sử dụng `ServerRpc` để xử lý logic bắn đạn/nhận sát thương và `ClientRpc` để kích hoạt các hiệu ứng hình ảnh (VFX, UI) đồng bộ trên tất cả người chơi.
* **Flexible Buff System:** Thiết kế hệ thống Power-up dựa trên kiến trúc Modular, dễ dàng mở rộng và thêm mới các loại hiệu ứng mà không làm ảnh hưởng đến logic lõi.

---

## Hình ảnh dự án (Screenshots)

| Lobby & Connection | Boss Fight (Laser Phase) | Victory Screen |
| :--- | :--- | :--- |
| ![Lobby](https://via.placeholder.com/300x200?text=Lobby+UI) | ![BossFight](https://via.placeholder.com/300x200?text=Boss+Laser+Phase) | ![Victory](https://via.placeholder.com/300x200?text=Victory+UI) |

---

## Hướng dẫn cài đặt & Chạy thử

### Yêu cầu:
* Windows 10/11.
* Unity Editor 6 (6000.4.0f1) hoặc mới hơn.

### Cài đặt:
1. Clone repository:
   ```bash
   git clone [https://github.com/Duyacquy/DualWings.git](https://github.com/Duyacquy/DualWings.git)

2. Mở dự án bằng Unity Hub.

3. Build dự án thành file .exe qua cửa sổ Build Settings.

4. Cách kết nối:
- Host (Người tạo phòng): Nhấn Create Room. IP máy bạn sẽ hiển thị trên màn hình.

- Client (Người tham gia): Nhập IP của Host vào ô Find Room và nhấn Join.

## Tác giả
- Họ và tên: Trần Ánh Duy

- Email: duytran2552005@gmail.com

- GitHub: Duyacquy
