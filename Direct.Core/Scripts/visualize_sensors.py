import json
import matplotlib.pyplot as plt

# Загрузка данных
with open("bfield_3d.json") as f:
    data = json.load(f)

# Извлечение данных
x = [d["x"] for d in data]
y = [d["y"] for d in data]
bx = [d["bx"] for d in data]
by = [d["by"] for d in data]
bz = [d["bz"] for d in data]

# Визуализация: цвет — Bz, стрелки — (bx, by)
plt.figure(figsize=(10, 8))
sc = plt.scatter(x, y, c=bz, cmap='seismic', s=80)
plt.colorbar(sc, label="B_z (T)")

# Стрелки
plt.quiver(x, y, bx, by, color='black', scale=5)

plt.title("Сенсоры: цвет = Bz, стрелки = (Bx, By)")
plt.xlabel("X")
plt.ylabel("Y")
plt.axis("equal")
plt.grid(True)
plt.tight_layout()
plt.show()
