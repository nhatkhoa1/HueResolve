# Import các thư viện cấu hình API
from fastapi import FastAPI
from pydantic import BaseModel
import random

app = FastAPI(title="HueResolve PhoBERT API", version="1.0")

# Định nghĩa cấu trúc dữ liệu nhận vào (Text của người dân)
class PredictionRequest(BaseModel):
    text: str

# Định nghĩa cấu trúc dữ liệu trả về (.NET sẽ hứng cái này)
class PredictionResponse(BaseModel):
    category_id: int
    confidence: float

@app.post("/api/predict", response_model=PredictionResponse)
def predict_category(request: PredictionRequest):
    # Logic tạm thời (Mock) để .NET có thể kết nối và test luồng dữ liệu
    # Trong thực tế, bạn sẽ truyền request.text vào model PhoBERT ở đây
    
    predicted_id = random.randint(1, 5)
    confidence_score = random.uniform(0.4, 0.98) 
    
    return PredictionResponse(
        category_id=predicted_id,
        confidence=round(confidence_score, 2)
    )