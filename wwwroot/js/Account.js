﻿async function initMap() {
    await ymaps3.ready;

    const { YMap, YMapDefaultSchemeLayer, YMapListener } = ymaps3;


    const map = new YMap(
        document.getElementById('map'),
        {
            location: {
                center: [49.668023, 58.603595],
                zoom: 10
            }
        }
    );

    const mapListener = new YMapListener({
        layer: 'any',
    })

    const behaviorEndHandler = ({ type }) => {
        if (type === 'drag') {
            const center = map.center;
            const address = geocode(center);
            console.log('Текущий адрес:', address);
        }
    };

    mapListener.update({ onActionEnd: behaviorEndHandler });

    map.addChild(mapListener);
    map.addChild(new YMapDefaultSchemeLayer());

    // Функция для геокодирования координат в адрес
    async function geocode(coords) {
        const response = await fetch(`https://geocode-maps.yandex.ru/1.x/?format=json&geocode=${coords[0]},${coords[1]}&apikey=46514e07-4df3-4059-9c68-309be409aa4b`);
        const data = await response.json();
        const address = data.response.GeoObjectCollection.featureMember[0].GeoObject.metaDataProperty.GeocoderMetaData.text;
        // Обновляем текст элемента с классом 'address-data'
        const elem = document.querySelector('.address-data'); // Используем querySelector для выбора элемента
        if (elem) {
            elem.textContent = `Адрес: ${address}`; // Используем обратные кавычки для интерполяции
        }
        return address;
    }
}

initMap();

$(document).on('click', '#send-address-data', function () {

    const elem = document.querySelector('.address-data');

    array = elem.textContent.split(',');

    $.ajax({
        url: '/Account/AddAddress',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ City: array[1], Street: array[2], House: array[3] }),
        success: function () {
            console.log('Адрес сохранен!');
        },
        error: function () {
            alert('Укажите правильный адресс доставки!');
        }
    });
});

$(document).on('click', '#delete-address-data', function () {

    const elem = document.querySelector('.Adress-id');

    $.ajax({
        url: '/Account/DeleteAddress',
        type: 'DELETE',
        data: { addressId: elem.textContent },
        success: function () {
            console.log('Адрес удален!');
        },
    });
});