import time
from flask import request
from pip._vendor.retrying import retry

from common_utils import APIKeyLoader, GeoUtil
from naver_scraper import NaverScraper  # 네이버 크롤러
import db_connector as db

host_info = APIKeyLoader.load('host_setting.dll')
n_scraper = NaverScraper()
db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])

@retry(stop_max_attempt_number=7, wait_random_min=1000)
def browse_restaurants(lon, lat, radius):
    '''
    BROWSE RESTAURANTS API
    GPS 좌표 기반으로 주변의 식당 정보 제공
    x: 경도 (그리니치 천문대 기준 동경 130도 정도)
    y: 위도 (적도 기준 북위 35~36도 정도)
    radius: 반경(m)
    :return: JSON 형태의 식당 리스트
    '''

    result_dict = {}

    # 네이버 플레이스 파싱
    result_dict['restaurants'] = n_scraper.scrape_place(lon, lat, radius)


    return result_dict
    #return jsonify(result_dict) #string 타입을 json형식으로 볼수있는 함수

slon = 126.992658
slat = 37.566350
slat =36.362374
slon = 127.335985
radius = 500
for i in range(100):
    rd = browse_restaurants(slon, slat, radius)
    slon = round(slon + GeoUtil.lon_delta * radius, 6)

    duplicate_check_query = "SELECT * FROM restaurants WHERE no = %s"


    for r in rd["restaurants"]:
        if len(db_conn.execute_all(duplicate_check_query, r['id'])) == 0:
            query = "INSERT INTO restaurants (no, name, category) VALUES (%s, %s, %s)"

            parameter = (r['id'], r['name'], r['category'])
            db_conn.execute(query, parameter)
            db_conn.commit()
    time.sleep(1)