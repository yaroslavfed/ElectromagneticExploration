import json

import matplotlib.pyplot as plt


# 📂 Загрузка сенсоров из JSON-файла
def load_sensors(filepath):
    with open(filepath, 'r') as f:
        return json.load(f)

# 📊 Построение 3D scatter-графика
def plot_sensors(sensors):
    x = [s["X"] for s in sensors]
    y = [s["Y"] for s in sensors]
    z = [s["Value"] for s in sensors]

    fig = plt.figure()
    ax = fig.add_subplot(111, projection='3d')

    sc = ax.scatter(x, y, z, c=z, cmap='viridis')

    ax.set_xlabel("X")
    ax.set_ylabel("Y")
    ax.set_zlabel("Value")
    plt.colorbar(sc, label=u'Δg')
    plt.title("Карта аномалий")

    plt.savefig('anomaly_chart.png', dpi=300, bbox_inches='tight')
    print(f"Сохранено изображение: {'anomaly_chart.png'}")
    #plt.show()

# 🚀 Точка входа
if __name__ == '__main__':
    sensor_file = 'anomaly_data.json'  # Путь к JSON-файлу
    sensors = load_sensors(sensor_file)
    plot_sensors(sensors)
