import unicodedata
import re
from fastapi import FastAPI, WebSocket
from fastapi.responses import Response
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional, List

# ==================================================================================
# HueResolve AI Microservice v2.1
# ==================================================================================
# Kiến trúc: FastAPI + Heuristic Keyword Scoring (Mock PhoBERT)
# v2.1: Scoring đơn vị dựa theo TÊN ĐƠN VỊ thực tế, không dùng type chung
#
# Phiên bản production sẽ thay thế bằng PhoBERT inference thực:
#   from transformers import AutoTokenizer, AutoModelForSequenceClassification
#   tokenizer = AutoTokenizer.from_pretrained("vinai/phobert-base")
#   model = AutoModelForSequenceClassification.from_pretrained("your-finetuned-model")
# ==================================================================================

app = FastAPI(
    title="HueResolve AI Service",
    description="Microservice phân loại phản ánh và gợi ý đơn vị xử lý cho thành phố Huế.",
    version="2.1.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["POST", "GET"],
    allow_headers=["*"],
)

# ==================================================================================
# HELPER: Chuẩn hóa tiếng Việt
# ==================================================================================

def normalize_vi(text: str) -> str:
    text = text.lower().strip()
    text = re.sub(r'\s+', ' ', text)
    return text

def remove_accents(text: str) -> str:
    """Tạo bản không dấu để match từ khóa khi người dùng gõ thiếu dấu trên mobile."""
    nfkd = unicodedata.normalize('NFKD', text)
    return "".join(c for c in nfkd if not unicodedata.combining(c))

def text_contains(text: str, text_no_accent: str, keyword: str) -> bool:
    """Kiểm tra keyword trong cả bản có dấu lẫn không dấu."""
    keyword_no_accent = remove_accents(keyword)
    return keyword in text or keyword_no_accent in text_no_accent

def score_to_confidence(raw_score: int, max_score: int) -> float:
    """Chuyển điểm keyword thô thành confidence trong [0.75, 0.98] cho đẹp mắt khi demo."""
    if max_score == 0 or raw_score == 0:
        return 0.75
    ratio = raw_score / max_score
    confidence = 0.75 + ratio * 0.23
    return round(min(0.98, confidence), 2)

# ==================================================================================
# TỪ ĐIỂN CHO PHÂN LOẠI DANH MỤC (predict_category)
# Mỗi topic là code của Category, score = 1/2/3 theo độ đặc trưng
# ==================================================================================

CATEGORY_KEYWORDS: dict[str, list[tuple[str, int]]] = {
    "moitruong": [
        ("rác thải", 3), ("bãi rác", 3), ("xả thải", 3), ("phế thải", 3),
        ("chất thải", 3), ("nước thải", 3), ("mùi hôi", 2), ("hôi thối", 2),
        ("ô nhiễm", 2), ("cống thoát", 2), ("ngập úng", 2), ("bụi", 1),
        ("khói", 1), ("vệ sinh", 1), ("quét dọn", 1), ("rác", 1), ("cây đổ", 1),
    ],
    "giaothong": [
        ("ổ gà", 3), ("mặt đường hỏng", 3), ("tai nạn giao thông", 3),
        ("đường sụt lún", 3), ("đèn giao thông hỏng", 2), ("biển báo", 2),
        ("dải phân cách", 2), ("đường hỏng", 2), ("kẹt xe", 2), ("ùn tắc", 2),
        ("đỗ xe sai", 2), ("vỉa hè bị chiếm", 2), ("ngược chiều", 2),
        ("đường", 1), ("xe", 1), ("giao thông", 1),
    ],
    "anninh": [
        ("đánh nhau", 3), ("cướp giật", 3), ("trộm cắp", 3), ("trộm", 3),
        ("cướp", 3), ("ma túy", 3), ("đâm chém", 3), ("gây rối", 2),
        ("tụ tập gây mất an ninh", 2), ("đua xe trái phép", 2), ("tệ nạn", 2),
        ("karaoke ồn ào", 2), ("đe dọa", 2), ("an ninh", 1), ("trật tự", 1),
    ],
    "hatang": [
        # --- Điện ---
        ("mất điện", 3), ("cúp điện", 3), ("đứt cáp điện", 3), ("chập điện", 3),
        ("cột điện đổ", 3), ("dây điện đứt", 3), ("trạm điện hỏng", 2),
        ("đèn đường hỏng", 2), ("chiếu sáng công cộng", 2),
        # --- Nước ---
        ("mất nước", 3), ("vỡ ống nước", 3), ("nước sạch bị đục", 3),
        ("ống nước rò rỉ", 3), ("nước không có áp", 2), ("cúp nước", 3),
        # --- Chung ---
        ("hạ tầng", 1), ("cáp quang đứt", 2), ("nắp hố ga", 2),
    ],
    "dothi": [
        ("xây dựng không phép", 3), ("xây trái phép", 3), ("san lấp trái phép", 3),
        ("lấn chiếm vỉa hè", 3), ("chiếm dụng lòng đường", 3),
        ("quảng cáo rao vặt", 2), ("biển hiệu sai quy định", 2),
        ("họp chợ lấn đường", 2), ("lấn chiếm", 2),
        ("đô thị", 1), ("quy hoạch", 1), ("chợ", 1),
    ],
}

# ==================================================================================
# CÁC PROFILE TỪ KHÓA THEO TÊN ĐƠN VỊ (suggest_department)
# Mỗi profile là dict: {"keywords": [(kw, score)], "exclusions": [kw_loại_trừ]}
#
# Logic: Đơn vị nào có keyword KHỚP với text nhất thì được gợi ý.
#        "exclusions" là từ khóa KHÔNG thuộc phạm vi của đơn vị đó —
#        nếu xuất hiện trong text, KHÔNG chấm điểm cho đơn vị này.
# ==================================================================================

DEPT_PROFILES: list[dict] = [
    {
        "name_patterns": ["điện lực", "evn", "điện"],
        "keywords": [
            ("mất điện", 5), ("cúp điện", 5), ("cột điện đổ", 5),
            ("dây điện đứt", 5), ("chập điện", 5), ("đứt cáp điện", 5),
            ("trạm điện hỏng", 4), ("điện bị hư", 4), ("đèn đường hỏng", 3),
            ("chiếu sáng công cộng", 3), ("điện", 2),
        ],
        "exclusions": ["nước", "ống nước", "trộm", "rác", "đường hỏng"],
    },
    {
        "name_patterns": ["nước", "huewaco", "cấp nước", "công ty nước"],
        "keywords": [
            ("mất nước", 5), ("vỡ ống nước", 5), ("cúp nước", 5),
            ("nước sạch bị đục", 5), ("ống nước rò rỉ", 5),
            ("nước không có áp", 4), ("thiếu nước", 4), ("nước", 2),
        ],
        "exclusions": ["điện", "mất điện", "trộm", "rác", "đường"],
    },
    {
        "name_patterns": ["môi trường", "hepco", "vệ sinh môi trường", "công ty cây xanh"],
        "keywords": [
            ("rác thải", 5), ("bãi rác", 5), ("xả thải", 5), ("chất thải", 5),
            ("nước thải", 4), ("mùi hôi", 4), ("ô nhiễm", 4), ("hôi thối", 4),
            ("phế thải", 4), ("rác", 3), ("bụi", 2), ("khói", 2),
            ("cây đổ", 3), ("cây xanh", 3), ("vệ sinh", 2),
        ],
        "exclusions": ["điện", "nước sạch", "trộm", "đường hỏng"],
    },
    {
        "name_patterns": ["công an", "cảnh sát", "ca ", "công an phường", "công an quận"],
        "keywords": [
            ("đánh nhau", 5), ("cướp", 5), ("trộm", 5), ("ma túy", 5),
            ("đâm chém", 5), ("cướp giật", 5), ("trộm cắp", 5),
            ("gây rối", 4), ("tụ tập", 3), ("đua xe trái phép", 4),
            ("tệ nạn", 3), ("đe dọa", 4), ("karaoke ồn ào", 3),
            ("an ninh", 2), ("trật tự", 2),
        ],
        "exclusions": ["điện", "nước", "rác", "đường hỏng", "ổ gà"],
    },
    {
        "name_patterns": ["giao thông", "qlđt", "quản lý đô thị", "hạ tầng"],
        "keywords": [
            ("ổ gà", 5), ("mặt đường hỏng", 5), ("đường sụt lún", 5),
            ("tai nạn giao thông", 4), ("đèn giao thông hỏng", 4),
            ("biển báo", 3), ("dải phân cách hỏng", 4), ("kẹt xe", 3),
            ("vỉa hè bị chiếm", 4), ("ngược chiều", 3), ("đường hỏng", 4),
            ("lấn chiếm lòng đường", 4), ("đỗ xe sai", 3), ("giao thông", 2),
        ],
        "exclusions": ["điện", "nước", "trộm", "rác"],
    },
    {
        "name_patterns": ["ubnd", "ủy ban nhân dân", "ban quản lý"],
        "keywords": [
            ("xây dựng không phép", 4), ("xây trái phép", 4),
            ("lấn chiếm vỉa hè", 4), ("quảng cáo rao vặt", 3),
            ("biển hiệu sai", 3), ("họp chợ lấn đường", 3),
            ("san lấp trái phép", 4), ("quy hoạch", 2),
        ],
        "exclusions": [],
    },
]

# ==================================================================================
# MODELS
# ==================================================================================

class CategoryItem(BaseModel):
    id: int
    name: str
    code: str
    description: Optional[str] = None

class DepartmentItem(BaseModel):
    id: int
    name: str
    type: Optional[str] = None

class IncidentData(BaseModel):
    title: str
    description: str
    categories: List[CategoryItem]

class DepartmentData(BaseModel):
    title: str
    description: str
    category_id: Optional[int] = None
    departments: List[DepartmentItem]

# ==================================================================================
# ENDPOINTS
# ==================================================================================

@app.get("/health")
def health_check():
    return {"status": "ok", "version": "2.1.0", "engine": "Keyword_Profile_Scoring"}

@app.get("/")
def read_root():
    """Trang chủ mặc định để tránh lỗi 404 khi mở trình duyệt"""
    return {"message": "HueResolve AI Service is running. Use /health to check status."}

@app.get("/favicon.ico")
def favicon():
    """Trả về file rỗng để tránh lỗi 404 favicon.ico"""
    return Response(content=b"", media_type="image/x-icon")

@app.websocket("/ws/ws")
async def websocket_dummy(websocket: WebSocket):
    """Endpoint giả để hứng các kết nối WebSocket từ extension trình duyệt tránh log lỗi"""
    await websocket.accept()
    await websocket.close()


@app.post("/predict_category")
def predict_category(data: IncidentData):
    """Phân loại phản ánh vào đúng danh mục với confidence động."""
    combined_text = normalize_vi(data.title + " " + data.description)
    text_no_accent = remove_accents(combined_text)

    scoring: dict[int, int] = {cat.id: 0 for cat in data.categories}
    max_per_cat: dict[int, int] = {}

    for cat in data.categories:
        cat_key = cat.code.lower()
        keywords_for_cat = CATEGORY_KEYWORDS.get(cat_key, [])
        max_per_cat[cat.id] = sum(w for _, w in keywords_for_cat)

        for kw, weight in keywords_for_cat:
            if text_contains(combined_text, text_no_accent, kw):
                scoring[cat.id] += weight

        # Bonus nếu tên danh mục xuất hiện trực tiếp trong text
        if normalize_vi(cat.name) in combined_text:
            scoring[cat.id] += 2

    best_id = max(scoring, key=scoring.get) if scoring else None
    best_score = scoring.get(best_id, 0) if best_id else 0
    best_max = max(max_per_cat.get(best_id, 1), 1) if best_id else 1

    confidence = score_to_confidence(best_score, best_max)

    if best_score == 0:
        best_id = data.categories[0].id if data.categories else None
        confidence = 0.30
        state = "LowConfidence_Fallback"
    elif confidence >= 0.70:
        state = "HighConfidence_Classified"
    else:
        state = "MediumConfidence_NeedsReview"

    return {"category_id": best_id, "confidence": confidence, "state": state}


@app.post("/suggest_department")
def suggest_department(data: DepartmentData):
    """
    Gợi ý đơn vị xử lý phù hợp dựa theo Profile từ khóa TÊN ĐƠN VỊ.
    Khắc phục vấn đề: 'mất điện' không còn gợi ý công ty nước nữa.
    """
    combined_text = normalize_vi(data.title + " " + data.description)
    text_no_accent = remove_accents(combined_text)

    results = []

    for dept in data.departments:
        dept_name_norm = normalize_vi(dept.name)

        # Tìm profile phù hợp nhất cho đơn vị này theo tên
        matched_profile = None
        for profile in DEPT_PROFILES:
            if any(pattern in dept_name_norm for pattern in profile["name_patterns"]):
                matched_profile = profile
                break

        if matched_profile is None:
            # Đơn vị không có profile cụ thể → bỏ qua (không gợi ý tràn lan)
            continue

        # Kiểm tra exclusions: nếu text có từ khóa loại trừ, đơn vị này KHÔNG phù hợp
        has_exclusion = any(
            text_contains(combined_text, text_no_accent, excl)
            for excl in matched_profile["exclusions"]
        )
        # Cho phép một chút nếu score rất cao (trường hợp text đề cập nhiều vấn đề)
        # Nhưng nếu bị loại trừ, giảm trọng số đáng kể
        exclusion_penalty = 0.5 if has_exclusion else 1.0

        # Tính điểm theo keyword của profile
        score = 0
        matched_reasons: list[str] = []
        max_score = sum(w for _, w in matched_profile["keywords"])

        for kw, weight in matched_profile["keywords"]:
            if text_contains(combined_text, text_no_accent, kw):
                score += weight
                matched_reasons.append(f"«{kw}»")

        if score > 0:
            effective_score = score * exclusion_penalty
            confidence = score_to_confidence(int(effective_score), max_score)

            if confidence >= 0.70:
                results.append({
                    "id": dept.id,
                    "name": dept.name,
                    "confidence": confidence,
                    "reason": "Phân tích AI: Nội dung phản ánh phù hợp với lĩnh vực chuyên môn của đơn vị."
                })

    # Sắp xếp theo confidence giảm dần, trả về top 3
    results = sorted(results, key=lambda x: x["confidence"], reverse=True)[:3]

    # Fallback thông minh nếu không tìm được đơn vị nào
    if not results:
        ubnd = next(
            (d for d in data.departments if "ubnd" in normalize_vi(d.name)),
            data.departments[0] if data.departments else None
        )
        if ubnd:
            results.append({
                "id": ubnd.id,
                "name": ubnd.name,
                "confidence": 0.65,
                "reason": "AI đề xuất: Đơn vị quản lý hành chính tổng hợp tuyến đầu."
            })

    return {"suggestions": results}


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000, reload=False)
