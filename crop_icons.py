import cv2
import os

input_path = r'C:/Users/fly20/.gemini/antigravity/brain/7dfd7b80-47eb-44b0-870c-e06ae17f4316/pcb_vision_icons_flat_white_1767406344397.png'
output_dir = r'd:/Repo/opencv/Resources/Icons'

if not os.path.exists(output_dir):
    os.makedirs(output_dir)

# Load image (with transparency)
img = cv2.imread(input_path, cv2.IMREAD_UNCHANGED)
h, w, _ = img.shape

# 4x4 grid
tile_h = h // 4
tile_w = w // 4

icon_names = [
    "New", "Open", "Save", "RunOnce",
    "RunLoop", "Stop", "Settings", "ZoomIn",
    "ZoomOut", "Fit", "Pointer", "RoiRect",
    "RoiCircle", "RoiPoly", "Undo", "Redo"
]

for i in range(4):
    for j in range(4):
        idx = i * 4 + j
        name = icon_names[idx]
        
        y1 = i * tile_h
        y2 = (i + 1) * tile_h
        x1 = j * tile_w
        x2 = (j + 1) * tile_w
        
        tile = img[y1:y2, x1:x2]
        
        # Resize to 32x32 for high quality UI
        tile_resized = cv2.resize(tile, (32, 32), interpolation=cv2.INTER_AREA)
        
        output_path = os.path.join(output_dir, f"{name}.png")
        cv2.imwrite(output_path, tile_resized)
        print(f"Saved {output_path}")
