import json

import matplotlib.pyplot as plt
import matplotlib.tri as tri
import numpy as np

# Загрузка данных
with open("field_data.json", "r") as f:
    data = json.load(f)

# Извлекаем координаты и значение |B|
x = [point["X"] for point in data]
y = [point["Y"] for point in data]
b_magnitude = [point["Magnitude"] for point in data]

# Создание триангуляции и интерполятора
triang = tri.Triangulation(x, y)
interpolator = tri.LinearTriInterpolator(triang, b_magnitude)

# Сетка для отображения
xi = np.linspace(min(x), max(x), 100)
yi = np.linspace(min(y), max(y), 100)
Xi, Yi = np.meshgrid(xi, yi)
Zi = interpolator(Xi, Yi)

# Визуализация
plt.figure(figsize=(8, 6))
contour = plt.contourf(Xi, Yi, Zi, levels=20, cmap='turbo')
plt.colorbar(contour, label='|B| (магнитное поле)')
plt.xlabel("X (м)")
plt.ylabel("Y (м)")
plt.title("Контурная карта магнитного поля |B| на поверхности")
plt.grid(True)
plt.show()
