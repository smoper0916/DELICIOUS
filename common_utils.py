import math, numbers

import requests


class APIKeyLoader:
    default_path = '../'

    @staticmethod
    def load(name):
        keys = []
        with open(APIKeyLoader.default_path + name, encoding='utf-8') as f:
            for line in f.readlines():
                k = line.split('|')
                if isinstance(k, list):
                    k = k[0]
                keys.append(k.strip())
        return keys


class GeoUtil:
    # 1m 기준 단위
    lon_delta = 0.0000111
    lat_delta = 0.000008983

    @staticmethod
    def degree2rad(degree):
        return degree * (math.pi / 180)

    @staticmethod
    def calc_distance(a, b):
        if type(a) is not tuple or type(b) is not tuple:
            return None
        elif len(a) != 2 or len(b) != 2:
            return None

        assert isinstance(a[0], numbers.Number) and -180 <= a[0] <= 180
        assert isinstance(a[1], numbers.Number) and -90 <= a[1] <= 90
        assert isinstance(b[0], numbers.Number) and -180 <= b[0] <= 180
        assert isinstance(b[1], numbers.Number) and -90 <= b[1] <= 90

        R = 6378.137  # 지구의 반경(단위: km)
        dLon = GeoUtil.degree2rad(b[0] - a[0])
        dLat = GeoUtil.degree2rad(b[1] - a[1])

        a = math.sin(dLat / 2) * math.sin(dLat / 2) \
            + (math.cos(GeoUtil.degree2rad(a[1])) \
               * math.cos(GeoUtil.degree2rad(b[1])) \
               * math.sin(dLon / 2) * math.sin(dLon / 2))
        b = 2 * math.atan2(math.sqrt(a), math.sqrt(1 - a))
        return round(R * b, 4)

    @staticmethod
    def get_bounds(lon, lat, radius):
        # return : [ min lon, min lat, max lon, max lat ]
        return [round(lon-GeoUtil.lon_delta*radius/2, 6), round(lat-GeoUtil.lat_delta*radius/2, 6), round(lon+GeoUtil.lon_delta*radius/2, 6), round(lat+GeoUtil.lat_delta*radius/2, 6)]


class KakaoUtil:
    api_key = ''

    @staticmethod
    def init():
        keys = APIKeyLoader.load('kakao_key.dll')
        KakaoUtil.api_key = keys[0] if len(keys) > 0 else ''

    @staticmethod
    def get_restaurants(lon, lat, radius):
        '''
            Kakao API Get Local Restaurants
        :param lon:
        :param lat:
        :param radius:
        :return: JSON Restaurants
        '''
        # kakao REST_API

        headers = {
            "Authorization": "KakaoAK " + KakaoUtil.api_key
        }
        page_num = 1
        doc_size = 15
        results = []

        while True:
            url = "https://dapi.kakao.com/v2/local/search/category.json?category_group_code=FD6&sort=distance&x=" + \
                  str(lon) + "&y=" + str(lat) + "&radius=" + str(radius) + "&page=" + str(page_num) + "&size=" + str(
                doc_size)

            response = requests.get(url, headers=headers)
            rescode = response.status_code

            if rescode == 200:
                json_data = response.json()
                for i in range(len(json_data["documents"])):
                    results.append(
                        {'name': str(json_data["documents"][i]['place_name']),
                         'lon': float(json_data["documents"][i]['x']),
                         'lat': float(json_data["documents"][i]['y'])})
                if json_data["meta"]["is_end"] is True:
                    break
                page_num += 1
            else:
                raise Exception('RequestFailed')

        return results

    @staticmethod
    def get_map_xy(addr):
        '''
        GET MAP API
        도로명주소를 위경도로 변환.
        addr: 도로명 주소
        :return: (경도, 위도)
        '''
        headers = {
            "Authorization": "KakaoAK " + KakaoUtil.api_key
        }
        url = "https://dapi.kakao.com/v2/local/search/address.json?query=" + addr

        response = requests.get(url, headers=headers)
        rescode = response.status_code

        if rescode == 200 and len(response.json()['documents']) > 0:
            json_data = response.json()

            return {'x': float(json_data["documents"][0]['x']),
                    'y': float(json_data["documents"][0]['y'])}
        else:
            return False



if __name__ == '__main__':
    gumi = (128.389535, 36.147779)
    pohang = (129.367652, 36.102002)
    seocheon = (126.611600, 36.102558)

    # 경도에 따른 변화 => 0.0111 = 1km
    lon_delta = 0.0000111
    dist = GeoUtil.calc_distance(gumi,(gumi[0]+lon_delta, gumi[1])) #구미
    print('구미', dist)
    dist = GeoUtil.calc_distance(pohang,(pohang[0]+lon_delta, pohang[1])) #포항
    print('포항', dist)
    dist = GeoUtil.calc_distance(seocheon,(seocheon[0]+lon_delta, seocheon[1])) #서천
    print('서천', dist)

    print()

    # 위도에 따른 변화
    lat_delta = 0.000008983
    seoul = (127.030789, 37.604082)
    daejeon = (127.357784, 36.336195)
    goheung = (127.335984, 34.625059)
    jeju = (126.611541, 33.423077)
    dist = GeoUtil.calc_distance(seoul, (seoul[0], seoul[1] + lat_delta))  # 서울
    print('서울', dist)
    dist = GeoUtil.calc_distance(daejeon, (daejeon[0], daejeon[1] + lat_delta))  # 대전
    print('대전', dist)
    dist = GeoUtil.calc_distance(goheung, (goheung[0], goheung[1] + lat_delta))  # 고흥
    print('고흥', dist)
    dist = GeoUtil.calc_distance(jeju, (jeju[0], jeju[1] + lat_delta))  # 제주
    print('제주', dist)

    bnds = GeoUtil.get_bounds(seoul[0], seoul[1], 200)
    print(bnds)