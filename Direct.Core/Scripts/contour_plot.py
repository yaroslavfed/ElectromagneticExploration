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
interpolators = {
    'mag': tri.LinearTriInterpolator(triang, b_magnitude),
    'bx': tri.LinearTriInterpolator(triang, bx),
    'by': tri.LinearTriInterpolator(triang, by)
}

# Сетка
xi = np.linspace(x.min(), x.max(), 100)
yi = np.linspace(y.min(), y.max(), 100)
Xi, Yi = np.meshgrid(xi, yi)

# Интерполяция данных
Zi = {key: interp(Xi, Yi) for key, interp in interpolators.items()}

# Визуализация
fig, axs = plt.subplots(2, 2, figsize=(14, 10), dpi=100)
fig.suptitle("Анализ магнитного поля", fontsize=14, y=1.02)

# Общие настройки для всех графиков
plot_config = {
    'mag': {'title': "Модуль поля (|B|)", 'cmap': 'viridis'},
    'bx': {'title': "X-компонента (Bx)", 'cmap': 'coolwarm'},
    'by': {'title': "Y-компонента (By)", 'cmap': 'coolwarm'}
}

# Графики плотности
for idx, (key, ax) in enumerate(zip(plot_config, axs.flatten()[:3])):
    cf = ax.contourf(Xi, Yi, Zi[key], levels=25,
                     cmap=plot_config[key]['cmap'], alpha=1)
    fig.colorbar(cf, ax=ax, label='', shrink=1)
    ax.set_title(plot_config[key]['title'], fontsize=10)
    ax.set(xlabel='X [м]', ylabel='Y [м]', aspect='equal')

# Настройки только для векторного поля
ax = axs[1, 1]
step = 6  # Уменьшаем шаг для большего количества стрелок

# Выборка данных
skip = (slice(None, None, step), slice(None, None, step))
U = Zi['bx'][skip]
V = Zi['by'][skip]
X_quiv = Xi[skip]
Y_quiv = Yi[skip]
M = np.hypot(U, V)

# Явное задание размеров (важно!)
min_arrow_size = 0.2  # Минимальный размер стрелки в метрах
max_arrow_size = 1   # Максимальный размер стрелки
arrow_scale = max_arrow_size / np.max(M)  # Масштабирующий коэффициент

# Нормализация и масштабирование
U_norm = U * arrow_scale
V_norm = V * arrow_scale

# Рисуем стрелки с фиксированным размером
quiv = ax.quiver(
    X_quiv, Y_quiv,
    U_norm, V_norm, M,
    angles='xy',
    scale_units='xy',
    scale=1.0,          # Отключаем авто-масштабирование
    width=0.008,        # Толщина в 2 раза больше
    headwidth=6,        # Гигантские головки
    headlength=7,
    headaxislength=5,
    cmap='turbo',         # Яркая цветовая схема
    edgecolor='black',  # Четкая обводка
    linewidth=0.5,
    alpha=0.95,
    zorder=10           # Выводим поверх других элементов
)

# Настройка цветовой шкалы
cbar = fig.colorbar(quiv, ax=ax, label='|B|')
cbar.ax.tick_params(labelsize=8)

# Фиксируем границы
ax.set_xlim(x.min(), x.max())
ax.set_ylim(y.min(), y.max())
ax.set_aspect('equal')
ax.set_title("Векторное поле: Bx и By")

plt.tight_layout()
plt.subplots_adjust(hspace=0.3, wspace=0.25)
plt.show()