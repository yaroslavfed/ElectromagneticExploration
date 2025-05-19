import json

import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d.art3d import Poly3DCollection


# 📂 Загрузка Mesh из файла
def load_mesh(filepath):
    with open(filepath, 'r') as f:
        data = json.load(f)
    return data['Cells']

# 🎨 Получение цвета по плотности
def density_to_color(density, min_d, max_d):
    norm = (density - min_d) / (max_d - min_d + 1e-9)
    return (0, 0, 0, norm)  # Черный цвет с разной прозрачностью

# 🔳 Генерация границ ячейки по центру и размерам
def get_box(center, bounds):
    cx, cy, cz = center
    dx, dy, dz = bounds

    # Вершины куба
    x = [cx - dx, cx + dx]
    y = [cy - dy, cy + dy]
    z = [cz - dz, cz + dz]

    # Грани
    return [
        [(x[0], y[0], z[0]), (x[1], y[0], z[0]), (x[1], y[1], z[0]), (x[0], y[1], z[0])],  # Нижняя
        [(x[0], y[0], z[1]), (x[1], y[0], z[1]), (x[1], y[1], z[1]), (x[0], y[1], z[1])],  # Верхняя
        [(x[0], y[0], z[0]), (x[0], y[1], z[0]), (x[0], y[1], z[1]), (x[0], y[0], z[1])],  # Левая
        [(x[1], y[0], z[0]), (x[1], y[1], z[0]), (x[1], y[1], z[1]), (x[1], y[0], z[1])],  # Правая
        [(x[0], y[0], z[0]), (x[1], y[0], z[0]), (x[1], y[0], z[1]), (x[0], y[0], z[1])],  # Передняя
        [(x[0], y[1], z[0]), (x[1], y[1], z[0]), (x[1], y[1], z[1]), (x[0], y[1], z[1])],  # Задняя
    ]

# 📊 Основная функция визуализации
def plot_mesh(cells):
    fig = plt.figure()
    ax = fig.add_subplot(111, projection='3d')

    densities = [cell["Density"] for cell in cells]
    min_d = min(densities)
    max_d = max(densities)

    for cell in cells:
        center = (cell["CenterX"], cell["CenterY"], cell["CenterZ"])
        bounds = (cell["BoundX"], cell["BoundY"], cell["BoundZ"])
        color = density_to_color(cell["Density"], min_d, max_d)

        box = get_box(center, bounds)
        cube = Poly3DCollection(box, facecolors=color, edgecolors='gray', linewidths=0.1)
        ax.add_collection3d(cube)

    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')

    ax.auto_scale_xyz(
        [min(cell["CenterX"] - cell["BoundX"] for cell in cells),
         max(cell["CenterX"] + cell["BoundX"] for cell in cells)],
        [min(cell["CenterY"] - cell["BoundY"] for cell in cells),
         max(cell["CenterY"] + cell["BoundY"] for cell in cells)],
        [min(cell["CenterZ"] - cell["BoundZ"] for cell in cells),
         max(cell["CenterZ"] + cell["BoundZ"] for cell in cells)]
    )

    plt.savefig('mesh_chart.png', dpi=300, bbox_inches='tight')
    print(f"Сохранено изображение: {'mesh_chart.png'}")
    #plt.show()

# 🚀 Запуск
if __name__ == '__main__':
    mesh_file = 'mesh_data.json'  # Путь к JSON-файлу
    cells = load_mesh(mesh_file)
    plot_mesh(cells)
