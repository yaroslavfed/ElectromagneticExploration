import argparse
import json
from dataclasses import dataclass
from typing import List, Optional

import matplotlib.colors as mcolors
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.cm import ScalarMappable
from matplotlib.patches import Polygon
from scipy.spatial import ConvexHull


@dataclass(frozen=True)
class Point3D:
    X: float
    Y: float
    Z: float


@dataclass(frozen=True)
class Sensor:
    Position: Point3D
    ComponentDirection: str


@dataclass(frozen=True)
class Node:
    NodeIndex: int
    Coordinate: Point3D


@dataclass
class Edge:
    EdgeIndex: int
    Nodes: List[Node]


@dataclass
class FiniteElement:
    Edges: List[Edge]
    Mu: float


def load_from_json(file_path: str) -> tuple[List[FiniteElement], List[Sensor]]:
    """Загрузка данных из JSON файла, созданного в C#"""
    try:
        with open(file_path, 'r') as f:
            data = json.load(f)
    except FileNotFoundError:
        raise ValueError(f"Файл {file_path} не найден")
    except json.JSONDecodeError:
        raise ValueError(f"Ошибка парсинга JSON в файле {file_path}")

    # Загрузка КЭ
    elements = []
    for element in data['Elements']:
        edges = []
        for edge_data in element['Edges']:
            nodes = []
            for node_data in edge_data['Nodes']:
                coord_data = node_data['Coordinate']
                node = Node(
                    NodeIndex=node_data['NodeIndex'],
                    Coordinate=Point3D(
                        X=coord_data['X'],
                        Y=coord_data['Y'],
                        Z=coord_data['Z']
                    )
                )
                nodes.append(node)
            edges.append(Edge(
                EdgeIndex=edge_data['EdgeIndex'],
                Nodes=nodes
            ))

        elements.append(FiniteElement(
            Edges=edges,
            Mu=element['Mu']
        ))

    # Загрузка сенсоров
    sensors = []
    for sensor_data in data['sensors']:
        pos_data = sensor_data['Position']
        sensors.append(Sensor(
            Position=Point3D(
                X=pos_data['X'],
                Y=pos_data['Y'],
                Z=pos_data['Z']
            ),
            ComponentDirection=sensor_data['ComponentDirection']
        ))

    if not elements:
        raise ValueError("Файл не содержит элементов для визуализации")

    return elements, sensors


def plot_finite_element_mesh(
        elements: List[FiniteElement],
        sensors: List[Sensor],
        x_slice: Optional[float] = None,
        y_slice: Optional[float] = None,
        z_slice: Optional[float] = None
):
    """Основная функция визуализации с поддержкой сечений и 2D проекций"""

    # Проверка входных данных
    if not elements:
        raise ValueError("Нет элементов для визуализации")

    # Настройка графиков с явным определением всех осей
    fig = plt.figure(figsize=(18, 12))
    gs = fig.add_gridspec(2, 2,
                          left=0.05, right=0.88,
                          top=0.95, bottom=0.05,
                          wspace=0.3, hspace=0.3
                          )

    # Всегда создаем все 4 оси
    ax3d = fig.add_subplot(gs[0, 0], projection='3d')
    ax_top_right = fig.add_subplot(gs[0, 1])  # Для XY или сечения Z
    ax_bottom_left = fig.add_subplot(gs[1, 0])  # Для XZ или сечения Y
    ax_bottom_right = fig.add_subplot(gs[1, 1])  # Для YZ или сечения X

    # Настройка цветовой карты
    mues = [el.Mu for el in elements]
    norm = plt.Normalize(min(mues), max(mues))
    cmap = plt.get_cmap('RdYlGn_r')
    mappable = ScalarMappable(norm=norm, cmap=cmap)

    # Сбор всех координат для расчета границ
    all_coords = []
    for element in elements:
        for edge in element.Edges:
            for node in edge.Nodes:
                all_coords.append((node.Coordinate.X, node.Coordinate.Y, node.Coordinate.Z))

    # Расчет границ с отступами
    padding = 0.1
    if all_coords:
        arr = np.array(all_coords)
        min_vals = arr.min(axis=0)
        max_vals = arr.max(axis=0)
    else:
        min_vals = [0 - padding, 0 - padding, 0 - padding]
        max_vals = [1 + padding, 1 + padding, 1 + padding]

    bounds = {
        'xy': {'x': (min_vals[0], max_vals[0]), 'y': (min_vals[1], max_vals[1])},
        'xz': {'x': (min_vals[0], max_vals[0]), 'y': (min_vals[2], max_vals[2])},
        'yz': {'x': (min_vals[1], max_vals[1]), 'y': (min_vals[2], max_vals[2])}
    }

    # 3D визуализация рёбер
    if sensors:
        sensor_coords = np.array([
            [s.Position.X, s.Position.Y, s.Position.Z]
            for s in sensors
        ])
        ax3d.scatter(
            sensor_coords[:, 0],
            sensor_coords[:, 1],
            sensor_coords[:, 2],
            c='red',
            marker='o',
            s=50,
            edgecolors='black',
            linewidths=1,
            label='Sensors',
            alpha=0.8
        )

        # Добавляем подписи для сенсоров
        for i, sensor in enumerate(sensors):
            ax3d.text(
                sensor.Position.X,
                sensor.Position.Y,
                sensor.Position.Z,
                f'S{i + 1}',
                color='darkred',
                fontsize=8,
                ha='center',
                va='bottom'
            )

        ax3d.legend(loc='upper right')

    for element in elements:
        color = cmap(norm(element.Mu))
        for edge in element.Edges:
            if len(edge.Nodes) != 2:
                continue
            n1, n2 = edge.Nodes
            ax3d.plot(
                [n1.Coordinate.X, n2.Coordinate.X],
                [n1.Coordinate.Y, n2.Coordinate.Y],
                [n1.Coordinate.Z, n2.Coordinate.Z],
                color=color, alpha=0.7, linewidth=1.5
            )

    # 3D оси подписи и заголовок
    ax3d.xaxis.set_pane_color((0.95, 0.95, 0.95, 0.1))
    ax3d.yaxis.set_pane_color((0.95, 0.95, 0.95, 0.1))
    ax3d.zaxis.set_pane_color((0.95, 0.95, 0.95, 0.1))
    ax3d.xaxis._axinfo["grid"].update({"linewidth": 0.5, "color": "gray"})
    ax3d.yaxis._axinfo["grid"].update({"linewidth": 0.5, "color": "gray"})
    ax3d.zaxis._axinfo["grid"].update({"linewidth": 0.5, "color": "gray"})

    ax3d.set_xlabel('X', fontsize=12, labelpad=15)
    ax3d.set_ylabel('Y', fontsize=12, labelpad=15)
    ax3d.set_zlabel('Z', fontsize=12, labelpad=15)

    ax3d.set_title('3D View', pad=20)

    # Функция для получения всех узлов элемента
    def get_element_nodes(element: FiniteElement) -> List[Node]:
        nodes = []
        for edge in element.Edges:
            for node in edge.Nodes:
                if node not in nodes:
                    nodes.append(node)
        return nodes

    # Функция для отрисовки 2D проекций с заливкой
    def draw_projection(ax, plane: str):
        """Отрисовка 2D проекции с заливкой элементов"""
        ax.cla()
        ax.set_title(f"{plane.upper()} Projection")
        ax.grid(True, linestyle='--', alpha=0.3)

        for element in elements:
            color = cmap(norm(element.Mu))
            nodes = get_element_nodes(element)

            # Проецируем узлы на плоскость
            coords = []
            for node in nodes:
                if plane == 'xy':
                    x, y = node.Coordinate.X, node.Coordinate.Y
                elif plane == 'xz':
                    x, y = node.Coordinate.X, node.Coordinate.Z
                else:  # yz
                    x, y = node.Coordinate.Y, node.Coordinate.Z
                coords.append((x, y))

            # Создаем выпуклую оболочку
            if len(coords) >= 3:
                try:
                    hull = ConvexHull(coords)
                    poly = Polygon(
                        np.array(coords)[hull.vertices],
                        closed=True,
                        facecolor=color,
                        edgecolor='k',
                        alpha=1
                    )
                    ax.add_patch(poly)
                except:
                    # Рисуем рёбра если не удалось построить оболочку
                    for edge in element.Edges:
                        if len(edge.Nodes) != 2: continue
                        n1, n2 = edge.Nodes
                        if plane == 'xy':
                            x1, y1 = n1.Coordinate.X, n1.Coordinate.Y
                            x2, y2 = n2.Coordinate.X, n2.Coordinate.Y
                        elif plane == 'xz':
                            x1, y1 = n1.Coordinate.X, n1.Coordinate.Z
                            x2, y2 = n2.Coordinate.X, n2.Coordinate.Z
                        else:
                            x1, y1 = n1.Coordinate.Y, n1.Coordinate.Z
                            x2, y2 = n2.Coordinate.Y, n2.Coordinate.Z
                        ax.plot([x1, x2], [y1, y2], color=color, linewidth=1)

        ax.set_xlim(bounds[plane]['x'])
        ax.set_ylim(bounds[plane]['y'])
        ax.set_aspect('equal')

    def invert_color(color):
        """Инвертирует цвет, принимая его в любом формате (название, HEX, RGB)"""
        rgb = mcolors.to_rgb(color)  # Преобразуем в RGB (0-1)
        inverted = (1 - rgb[0], 1 - rgb[1], 1 - rgb[2])  # Инверсия
        return inverted  # Возвращаем инвертированный цвет

    # Функция для отрисовки сечений
    def draw_slice(ax, axis: str, position: float):
        """Отрисовка сечения на указанной оси"""
        ax.cla()
        ax.set_title(f"Сечение по {axis}={position:.2f}")
        ax.grid(True, linestyle='dotted', alpha=0.5)

        for element in elements:
            color = cmap(norm(element.Mu))
            points = []
            for edge in element.Edges:
                if len(edge.Nodes) != 2:
                    continue
                n1, n2 = edge.Nodes
                coord1 = getattr(n1.Coordinate, axis)
                coord2 = getattr(n2.Coordinate, axis)

                if (coord1 <= position <= coord2) or (coord2 <= position <= coord1):
                    t = (position - coord1) / (coord2 - coord1 + 1e-9)
                    x = n1.Coordinate.X + t * (n2.Coordinate.X - n1.Coordinate.X)
                    y = n1.Coordinate.Y + t * (n2.Coordinate.Y - n1.Coordinate.Y)
                    z = n1.Coordinate.Z + t * (n2.Coordinate.Z - n1.Coordinate.Z)

                    if axis == 'X':
                        points.append((y, z))
                    elif axis == 'Y':
                        points.append((x, z))
                    else:
                        points.append((x, y))

            if len(points) >= 3:
                try:
                    hull = ConvexHull(points)
                    poly = Polygon(
                        np.array(points)[hull.vertices],
                        closed=True,
                        facecolor=color,
                        edgecolor=invert_color(color),
                        alpha=1
                    )
                    ax.add_patch(poly)
                except:
                    continue

        ax.autoscale_view()
        ax.set_aspect('equal')

    # Настройка отображения с явным управлением осями
    if x_slice is not None:
        draw_slice(ax_bottom_right, 'X', x_slice)
        ax_bottom_right.set(xlabel='Y', ylabel='Z')
    else:
        draw_projection(ax_bottom_right, 'yz')
        ax_bottom_right.set(xlabel='Y', ylabel='Z')

    if y_slice is not None:
        draw_slice(ax_bottom_left, 'Y', y_slice)
        ax_bottom_left.set(xlabel='X', ylabel='Z')
    else:
        draw_projection(ax_bottom_left, 'xz')
        ax_bottom_left.set(xlabel='X', ylabel='Z')

    if z_slice is not None:
        draw_slice(ax_top_right, 'Z', z_slice)
        ax_top_right.set(xlabel='X', ylabel='Y')
    else:
        draw_projection(ax_top_right, 'xy')
        ax_top_right.set(xlabel='X', ylabel='Y')

    # Цветовая шкала
    cbar_ax = fig.add_axes([0.90, 0.15, 0.02, 0.7])
    fig.colorbar(mappable, cax=cbar_ax, label='Mu')

    plt.savefig("graph.png", dpi=300)
    plt.show()


if __name__ == "__main__":
    # Настройка параметров командной строки
    parser = argparse.ArgumentParser(
        description='Визуализатор сетки конечных элементов',
        formatter_class=argparse.ArgumentDefaultsHelpFormatter
    )
    parser.add_argument(
        '-f', '--file',
        default='mesh_data.json',
        help='Путь к JSON файлу с данными'
    )
    parser.add_argument(
        '-x', '--x-slice',
        type=float,
        help='Позиция сечения по оси X'
    )
    parser.add_argument(
        '-y', '--y-slice',
        type=float,
        help='Позиция сечения по оси Y'
    )
    parser.add_argument(
        '-z', '--z-slice',
        type=float,
        help='Позиция сечения по оси Z'
    )

    args = parser.parse_args()

    try:
        elements, sensors = load_from_json("mesh_data.json")
        plot_finite_element_mesh(
            elements=elements,
            sensors=sensors,
            x_slice=7,
            y_slice=0,
            z_slice=-15
        )
    except Exception as e:
        print(f"\nОшибка: {str(e)}")
        exit(1)
