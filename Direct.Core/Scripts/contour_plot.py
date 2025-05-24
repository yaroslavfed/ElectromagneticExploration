import json

import matplotlib.pyplot as plt
import matplotlib.tri as tri
import numpy as np

# Загрузка данных
with open("field_data.json", "r") as f:
    data = json.load(f)

# Извлекаем данные
x = np.array([p["X"] for p in data])
y = np.array([p["Y"] for p in data])
bx = np.array([p["Bx"] for p in data])
by = np.array([p["By"] for p in data])
b_magnitude = np.array([p["Magnitude"] for p in data])

# Триангуляция и интерполяция
triang = tri.Triangulation(x, y)
interp_mag = tri.LinearTriInterpolator(triang, b_magnitude)
interp_bx = tri.LinearTriInterpolator(triang, bx)
interp_by = tri.LinearTriInterpolator(triang, by)

# Сетка
xi = np.linspace(min(x), max(x), 100)
yi = np.linspace(min(y), max(y), 100)
Xi, Yi = np.meshgrid(xi, yi)

Zi_mag = interp_mag(Xi, Yi)
Zi_bx = interp_bx(Xi, Yi)
Zi_by = interp_by(Xi, Yi)

# Визуализация
fig, axs = plt.subplots(2, 2, figsize=(14, 8))
fig.suptitle("Электромагнитное поле (все компоненты + модуль)")

# |B|
cf0 = axs[0, 0].contourf(Xi, Yi, Zi_mag, levels=20, cmap="viridis")
plt.colorbar(cf0, ax=axs[0, 0], label="|B|")
axs[0, 0].set_title("|B| — Модуль магнитного поля")
axs[0, 0].set_xlabel("X")
axs[0, 0].set_ylabel("Y")

# Bx
cf1 = axs[0, 1].contourf(Xi, Yi, Zi_bx, levels=20, cmap="coolwarm")
plt.colorbar(cf1, ax=axs[0, 1], label="Bx")
axs[0, 1].set_title("Bx — компонента по X")
axs[0, 1].set_xlabel("X")
axs[0, 1].set_ylabel("Y")

# By
cf2 = axs[1, 0].contourf(Xi, Yi, Zi_by, levels=20, cmap="coolwarm")
plt.colorbar(cf2, ax=axs[1, 0], label="By")
axs[1, 0].set_title("By — компонента по Y")
axs[1, 0].set_xlabel("X")
axs[1, 0].set_ylabel("Y")

# Векторное поле (направление + длина по модулю)
step = 8
U = Zi_bx[::step, ::step]
V = Zi_by[::step, ::step]
Xq = Xi[::step, ::step]
Yq = Yi[::step, ::step]

# Нормировка (по длине стрелок)
magnitude = np.sqrt(U**2 + V**2)
U_scaled = U / np.max(magnitude)
V_scaled = V / np.max(magnitude)

axs[1, 1].quiver(Xq, Yq, U_scaled, V_scaled, magnitude, angles="xy", scale=0.2, cmap="plasma")
axs[1, 1].set_title("Векторное поле (Bx, By)")
axs[1, 1].set_xlabel("X")
axs[1, 1].set_ylabel("Y")
axs[1, 1].set_xlim(x.min(), x.max())
axs[1, 1].set_ylim(y.min(), y.max())

plt.tight_layout()
plt.show()
