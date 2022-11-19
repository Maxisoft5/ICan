"""
Скрипт меняет размер картинок в карусели и добавляет рамку 
к картинке-обложке на странице тетрадки на сайте yanmogu.ru
Рекурсивно проходит все каталоги в заданной папке и обрабатывает изображения с заданными названиями (переменная DIR_NAME)
Если была добавлена новая тетрадка, для нее в каталоге celcel\public\img\product будет создана папка с номером 
Номер можно узнать из адреса картинки на сайте, например: http://yamogu.ru/img/product/30/0/full.jpg, - для этой тетрадки все картинки лежат в папке 30
Для того, чтобы подправить для нее изображения, забрать папку, запустить по ней скрипт и залить ее обратно на сервер

"""

import os
from PIL import Image


# Изменить на путь к каталогу где лежат изображения
DIR_NAME = "C:\code\py\product"
# DIR_NAME = "C:\\Users\\nata\\Dropbox\\work\\YaMogu\\Site-scripts"

# названия изображений для предпросмотра в карусели (сделаем файл thumb.jpg больше)
INPUT_IMAGE_SCALE = "original.jpg"
OUTPUT_IMAGE_SCALE = "thumb.jpg"
# новая ширина thumb.jpg
WIDTH = 500
# названия изображений обложки (к ним добавляем белую рамку)
INPUT_IMAGE_BORDER = "full.jpg"
OUTPUT_IMAGE_BORDER = "full.jpg"

"""
def make_border(input_image_path,
                output_image_path):
    original_image = Image.open(input_image_path)
    original_size = original_image.size
# создаем белый фон размером чуть побольше чем оригинальное изображение
    new_size = (int(original_size[0]*1.1), int(original_size[1]*1.1))
    new_image = Image.new("RGB", new_size, (255,250,250))
# считаем ширину рамки и вставляем оригинальное изображение на фон
    one_side_border = int((new_size[0]-original_size[0])/2)
    second_side_border = int((new_size[1]-original_size[1])/2)
    new_image.paste(original_image, (one_side_border, second_side_border))
    new_image.save(output_image_path)
"""


def make_border(input_image_path, output_image_path):
    original_image = Image.open(input_image_path)
    original_size = original_image.size
    new_size = (int(original_size[0] * 1.1), int(original_size[1] * 1.17))
    new_image = Image.new("RGB", new_size, (255, 250, 250))
    print("new size" + str(new_size))
    print("original_size" + str(original_size))
    print(
        "one side border "
        + str(int((new_size[0] - original_size[0]) / 2))
        + " second side border "
        + str((new_size[1] - original_size[1]) / 2)
    )
    one_side_border = int((new_size[0] - original_size[0]) / 2)
    second_side_border = int((new_size[1] - original_size[1]) / 2)
    second_side_border = int(
        second_side_border * 0.6
    )  # нижнюю рамочку сделаем пошире, чтобы не обрезалось лищнее на мобильном
    new_image.paste(original_image, (one_side_border, second_side_border))
    new_image.save(output_image_path)


def scale_image(input_image_path, output_image_path, width=None, height=None):
    original_image = Image.open(input_image_path)
    w, h = original_image.size
    # для масштабирования изображения должны быть заданы высота и ширина, или один из параметров
    if width and height:
        max_size = (width, height)
    elif width:
        max_size = (width, h)
    elif height:
        max_size = (w, height)
    else:
        raise RuntimeError("Width or height required!")
    # Масштабируем изображение
    original_image.thumbnail(max_size, Image.ANTIALIAS)
    original_image.save(output_image_path)


def handle_dirs_recurcive(dir_name):
    names = os.listdir(dir_name)
    for name in names:
        fullname = os.path.join(
            dir_name, name
        )  # получаем полный путь к файлу или папке
        if os.path.isfile(fullname):
            if name == INPUT_IMAGE_SCALE:
                full_outputname = os.path.join(dir_name, OUTPUT_IMAGE_SCALE)
                scale_image(fullname, full_outputname, WIDTH)
            elif name == INPUT_IMAGE_BORDER:
                full_outputname = os.path.join(dir_name, OUTPUT_IMAGE_BORDER)
                scale_image(fullname, full_outputname, WIDTH)
                # make_border(fullname, full_outputname)
        else:
            handle_dirs_recurcive(fullname)


if __name__ == "__main__":
    handle_dirs_recurcive(DIR_NAME)
