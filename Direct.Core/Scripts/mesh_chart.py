import json
import sys

import matplotlib.pyplot as plt

json_file = sys.argv[1]
output_image = sys.argv[2]

# Читаем данные
with open(json_file, "r") as f:
    data = json.load(f)

domain = data["domain"]
strata = data["strata"]
projection = data.get("projection", "XY")  # По умолчанию XY

fig, ax = plt.subplots(figsize=(8, 6))

# Определяем оси и метки
if projection == "XY":
    x_start, x_end = domain["StartX"], domain["EndX"]
    y_start, y_end = domain["StartY"], domain["EndY"]
    x_label, y_label = "X", "Y"
elif projection == "XZ":
    x_start, x_end = domain["StartX"], domain["EndX"]
    y_start, y_end = domain["StartZ"], domain["EndZ"]
    x_label, y_label = "X", "Z"
elif projection == "YZ":
    x_start, x_end = domain["StartY"], domain["EndY"]
    y_start, y_end = domain["StartZ"], domain["EndZ"]
    x_label, y_label = "Y", "Z"
else:
    raise ValueError(f"Неизвестная проекция: {projection}")

# Настраиваем оси
ax.set_xticks([x_start + i * (x_end - x_start) / domain["SplitsXCount"] for i in range(domain["SplitsXCount"] + 1)])
ax.set_yticks([y_start + i * (y_end - y_start) / domain["SplitsYCount"] for i in range(domain["SplitsYCount"] + 1)])
ax.grid(True, linestyle="--", linewidth=0.5, alpha=0.7)

# Границы области
rect = plt.Rectangle((x_start, y_start), x_end - x_start, y_end - y_start, linewidth=2, edgecolor='r', facecolor='none')
ax.add_patch(rect)

print(f"Проекция: {projection}, Domain: {domain}")

# Отрисовка слоёв
for stratum in strata:
    if projection == "XY":
        x_min, x_max = stratum["StartX"], stratum["EndX"]
        y_min, y_max = stratum["StartY"], stratum["EndY"]
    elif projection == "XZ":
        x_min, x_max = stratum["StartX"], stratum["EndX"]
        y_min, y_max = stratum["StartZ"], stratum["EndZ"]
    elif projection == "YZ":
        x_min, x_max = stratum["StartY"], stratum["EndY"]
        y_min, y_max = stratum["StartZ"], stratum["EndZ"]

    is_active = stratum.get("IsActive", True)
    alpha = 1.0 if is_active else 0.5

    # Обрезаем прямоугольник по границам области
    x_min = max(x_min, x_start)
    y_min = max(y_min, y_start)
    x_max = min(x_max, x_end)
    y_max = min(y_max, y_end)

    width = x_max - x_min
    height = y_max - y_min

    print(f"Stratum: Start=({x_min},{y_min}), End=({x_max},{y_max}), Active={is_active}")

    # Рисуем обрезанный прямоугольник
    obj_rect = plt.Rectangle((x_min, y_min), width, height, linewidth=1.5, edgecolor='blue', facecolor='lightblue', alpha=alpha)
    ax.add_patch(obj_rect)

    ax.text((x_min + x_max) / 2, (y_min + y_max) / 2, f"{stratum['Density']:.1f}", fontsize=8, ha='center', va='center', color='black')

ax.set_xlim(x_start, x_end)
ax.set_ylim(y_start, y_end)
ax.set_xlabel(x_label)
ax.set_ylabel(y_label)

plt.savefig(output_image, dpi=300, bbox_inches='tight')
print(f"Сохранено изображение: {output_image}")
