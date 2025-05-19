import json

import matplotlib.pyplot as plt
import numpy as np
from matplotlib.patches import Rectangle
from matplotlib.widgets import Button


def load_mesh(filepath):
    try:
        with open(filepath, 'r') as f:
            data = json.load(f)
        return data.get('Cells', [])
    except Exception as e:
        print(f"Ошибка загрузки файла: {e}")
        return []

class InteractiveSliceViewer:
    def __init__(self, cells):
        if not cells:
            print("Нет данных для визуализации!")
            return

        self.cells = cells
        self.fig = plt.figure(figsize=(18, 8))

        # Рассчет границ
        self.x_bounds = self._calculate_bounds('X')
        self.y_bounds = self._calculate_bounds('Y')
        self.z_bounds = self._calculate_bounds('Z')

        # Инициализация срезов
        self.current_slice = {
            'x': np.mean(self.x_bounds),
            'y': np.mean(self.y_bounds),
            'z': np.mean(self.z_bounds)
        }

        # Создание графиков
        self.ax_3d = self.fig.add_subplot(144, projection='3d')
        self.ax_xy = self.fig.add_subplot(141)
        self.ax_yz = self.fig.add_subplot(142)
        self.ax_xz = self.fig.add_subplot(143)

        self._create_controls()
        self.update_all_plots()
        plt.show()

    def _calculate_bounds(self, axis):
        min_val = max_val = None
        for cell in self.cells:
            center = cell[f"Center{axis}"]
            bound = cell[f"Bound{axis}"]
            low = center - bound
            high = center + bound
            if min_val is None or low < min_val:
                min_val = low
            if max_val is None or high > max_val:
                max_val = high
        return (min_val, max_val) if min_val is not None else (0, 1)

    def _create_controls(self):
        plt.subplots_adjust(left=0.1, right=0.9, bottom=0.25, top=0.95)
        self.buttons = {
            'x+': Button(plt.axes([0.15, 0.15, 0.1, 0.05]), 'X+'),
            'x-': Button(plt.axes([0.05, 0.15, 0.1, 0.05]), 'X-'),
            'y+': Button(plt.axes([0.15, 0.05, 0.1, 0.05]), 'Y+'),
            'y-': Button(plt.axes([0.05, 0.05, 0.1, 0.05]), 'Y-'),
            'z+': Button(plt.axes([0.15, 0.25, 0.1, 0.05]), 'Z+'),
            'z-': Button(plt.axes([0.05, 0.25, 0.1, 0.05]), 'Z-')
        }

        for btn in self.buttons.values():
            btn.label.set_fontsize(8)

        self.buttons['x+'].on_clicked(lambda e: self.adjust_slice('x', 1))
        self.buttons['x-'].on_clicked(lambda e: self.adjust_slice('x', -1))
        self.buttons['y+'].on_clicked(lambda e: self.adjust_slice('y', 1))
        self.buttons['y-'].on_clicked(lambda e: self.adjust_slice('y', -1))
        self.buttons['z+'].on_clicked(lambda e: self.adjust_slice('z', 1))
        self.buttons['z-'].on_clicked(lambda e: self.adjust_slice('z', -1))

    def adjust_slice(self, axis, direction):
        step = (self.__getattribute__(f'{axis}_bounds')[1] -
                self.__getattribute__(f'{axis}_bounds')[0]) / 20
        new_val = self.current_slice[axis] + direction * step
        self.current_slice[axis] = np.clip(new_val, *self.__getattribute__(f'{axis}_bounds'))
        self.update_all_plots()

    def _filter_cells(self, axis, value):
        return [cell for cell in self.cells
                if (cell[f"Center{axis}"] - cell[f"Bound{axis}"] <= value <=
                    cell[f"Center{axis}"] + cell[f"Bound{axis}"])]

    def _set_square_aspect(self, ax, x_range, y_range):
        """Устанавливает квадратное соотношение осей с разными диапазонами"""
        ax.set_box_aspect(y_range / x_range)  # Для matplotlib >= 3.3.0
        # Или для более старых версий:
        # ax.set_aspect(y_range / x_range, adjustable='datalim')

    def _plot_projection(self, ax, cells, x_axis, y_axis, fixed_axis):
        ax.clear()

        if not cells:
            ax.text(0.5, 0.5, 'Нет данных', ha='center', va='center')
            return

        # Рассчет диапазонов для осей
        x_min, x_max = self.__getattribute__(f"{x_axis.lower()}_bounds")
        y_min, y_max = self.__getattribute__(f"{y_axis.lower()}_bounds")
        x_range = x_max - x_min
        y_range = y_max - y_min

        # Установка квадратного соотношения
        self._set_square_aspect(ax, x_range, y_range)

        densities = [c["Density"] for c in cells]
        min_d, max_d = min(densities), max(densities)
        range_d = max_d - min_d if max_d != min_d else 1.0

        for cell in cells:
            x = cell[f"Center{x_axis}"] - cell[f"Bound{x_axis}"]
            y = cell[f"Center{y_axis}"] - cell[f"Bound{y_axis}"]
            width = 2 * cell[f"Bound{x_axis}"]
            height = 2 * cell[f"Bound{y_axis}"]

            color_value = 1 - (cell["Density"] - min_d) / range_d
            rect = Rectangle((x, y), width, height,
                             edgecolor='k', facecolor=plt.cm.gray(color_value), alpha=0.7)
            ax.add_patch(rect)

        ax.set_xlabel(x_axis)
        ax.set_ylabel(y_axis)
        ax.set_title(f"{x_axis}{y_axis} Срез ({fixed_axis} = {self.current_slice[fixed_axis.lower()]:.2f})")
        ax.set_xlim(x_min, x_max)
        ax.set_ylim(y_min, y_max)
        ax.grid(True)

    def _plot_3d(self):
        self.ax_3d.clear()
        densities = [c["Density"] for c in self.cells]
        min_d, max_d = min(densities), max(densities)

        for cell in self.cells:
            x = cell["CenterX"]
            y = cell["CenterY"]
            z = cell["CenterZ"]
            dx = cell["BoundX"] * 2
            dy = cell["BoundY"] * 2
            dz = cell["BoundZ"] * 2

            color_value = 1 - (cell["Density"] - min_d) / (max_d - min_d)
            color = plt.cm.gray(color_value)

            self.ax_3d.bar3d(x - cell["BoundX"], y - cell["BoundY"], z - cell["BoundZ"],
                             dx, dy, dz, color=color, alpha=0.3, edgecolor='k')

        self.ax_3d.set_xlim(*self.x_bounds)
        self.ax_3d.set_ylim(*self.y_bounds)
        self.ax_3d.set_zlim(*self.z_bounds)
        self.ax_3d.set_xlabel('X')
        self.ax_3d.set_ylabel('Y')
        self.ax_3d.set_zlabel('Z')
        self.ax_3d.set_title('3D View')

    def update_all_plots(self):
        # Обновление 2D проекций
        self._plot_projection(self.ax_xy, self._filter_cells('Z', self.current_slice['z']), 'X', 'Y', 'Z')
        self._plot_projection(self.ax_yz, self._filter_cells('X', self.current_slice['x']), 'Y', 'Z', 'X')
        self._plot_projection(self.ax_xz, self._filter_cells('Y', self.current_slice['y']), 'X', 'Z', 'Y')

        # Обновление 3D вида
        self._plot_3d()

        self.fig.canvas.draw_idle()

if __name__ == '__main__':
    cells = load_mesh('inverse.json')
    if cells:
        InteractiveSliceViewer(cells)
    else:
        print("Ошибка: Не удалось загрузить данные")