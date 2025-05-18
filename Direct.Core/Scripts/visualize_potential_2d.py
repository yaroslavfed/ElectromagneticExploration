import json
import matplotlib.pyplot as plt
from matplotlib.widgets import Button
import numpy as np

# === Параметры ===
Z_TOLERANCE = 1e-6  # Насколько близко по z считать "одним уровнем"

# === Загрузка данных ===
with open("solution.json", "r") as f:
    data = json.load(f)

# === Преобразование в массивы ===
points = np.array([[d["x"], d["y"], d["z"]] for d in data])
directions = np.array([[d["dx"], d["dy"], d["dz"]] for d in data])
values = np.array([d["value"] for d in data])

z_levels = sorted(np.unique(points[:, 2]))
z_index = 0  # индекс текущего z-уровня

# === Функция для фильтрации и отображения ===
def plot_z_level(index):
    global quiver
    z = z_levels[index]
    mask = np.abs(points[:, 2] - z) < Z_TOLERANCE
    x = points[mask, 0]
    y = points[mask, 1]
    u = directions[mask, 0] * values[mask]
    v = directions[mask, 1] * values[mask]

    ax.clear()
    ax.set_title(f"Срез по Z = {z:.2f}")
    ax.set_xlabel("X")
    ax.set_ylabel("Y")
    ax.quiver(x, y, u, v, values[mask], cmap="grey", scale=1, scale_units='xy')
    fig.canvas.draw_idle()

# === Обработчики кнопок ===
def next_z(event):
    global z_index
    if z_index < len(z_levels) - 1:
        z_index += 1
        plot_z_level(z_index)

def prev_z(event):
    global z_index
    if z_index > 0:
        z_index -= 1
        plot_z_level(z_index)

# === Построение интерфейса ===
fig, ax = plt.subplots()
plt.subplots_adjust(bottom=0.2)

# Кнопка вверх
ax_next = plt.axes([0.8, 0.05, 0.1, 0.075])
btn_next = Button(ax_next, '↑ Z')
btn_next.on_clicked(next_z)

# Кнопка вниз
ax_prev = plt.axes([0.65, 0.05, 0.1, 0.075])
btn_prev = Button(ax_prev, '↓ Z')
btn_prev.on_clicked(prev_z)

plot_z_level(z_index)  # начальный срез
plt.show()
