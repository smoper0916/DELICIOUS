from bs4 import BeautifulSoup
import requests
from flask import json
# import multiprocessing
from selenium.common.exceptions import NoSuchElementException
from selenium.webdriver.common.keys import Keys

#from common_utils import GeoUtil
from common_utils import *
import time
from selenium import webdriver

host_info = APIKeyLoader.load('host_setting.dll')

if len(host_info) < 1:
    print('Please Check your host info. It should be placed at ../host_setting.dll')
elif len(host_info) < 3:
    print("There's no DB Setting in host setting file. Please Check file again. path = ../host_setting.dll")

import db_connector as db

object = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])



class NaverScraper:
    driver_path = r'C:\Users\DeepLearning_3\Documents\ChromeDriver\chromedriver.exe'
    options = webdriver.ChromeOptions()

    def __init__(self):
        # headless 옵션 설정
        self.options.add_argument('headless')
        self.options.add_argument("no-sandbox")

        # 브라우저 윈도우 사이즈
        self.options.add_argument('window-size=1920x1080')

        # 사람처럼 보이게 하는 옵션들
        self.options.add_argument("disable-gpu")  # 가속 사용 x
        self.options.add_argument("lang=ko_KR")  # 가짜 플러그인 탑재
        self.options.add_argument(
            "user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36")

        self.driver = webdriver.Chrome(self.driver_path, chrome_options=self.options)

    def scrape_information(self, id):
        driver = self.driver
        info_dict = {}
        base_url = "https://store.naver.com/restaurants/detail?id=" + str(id)
        start = time.time()
        driver.get(base_url)
        driver_time = time.time()

        # global soup
        soup = BeautifulSoup(driver.page_source, 'html.parser')

        # nlink = soup.find("a", {"class": "btn"}) # 네이버 예약 링크
        # if nlink is not None and 'booking.naver.com' not in nlink.attrs['href']:
        #     nlink = None
        desc_area = soup.find("div", {"class": "list_item list_item_desc"})
        if desc_area is not None:
            btn_more = desc_area.find("a", {"class": "btn_more", "aria-label": "펼쳐보기"})
            if btn_more is not None:
                driver.find_element_by_xpath('//div/a[@aria-label="펼쳐보기"]').click()
                soup = BeautifulSoup(driver.page_source, 'html.parser')
        bizinfo_area = soup.find("div", {"class": "bizinfo_area"})  # 모든 기본 정보가 담긴 div
        # biz_message_area = soup.find("div", {"class": "default_info_area biz_message_area"})  # N페이 예약 혜택

        '''
            biztime : 영업시간 / telephone : 전화번호 / addr : 도로명주소 / homepage : 홈페이지 / convenience : 편의시설
            tv_history : TV 출연내역 / desc : 식당 설명
        '''
        for key_name, (tag_name, tag_class) in zip(
                ["biztime", "telephone", "addr", "convenience", "tv_history", "desc"],
                [("div", "biztime_row"), ("div", "list_item list_item_biztel"), ("span", "addr"),
                 ("div", "convenience"), ("div", "list_item list_item_tv"),
                 ("div", "list_item list_item_desc"), ]):
            parsed = bizinfo_area.find(tag_name, {"class": tag_class})  # Business Time
            if parsed is not None:
                info_dict[key_name] = parsed.text

        # Not None Value 채워넣기
        # for key_name, value in [('nlink', nlink), ('npay_msg', biz_message_area)]:
        #     if value is not None:
        #         info_dict[key_name] = value.attrs['href'] if key_name == 'nlink' else value.text
        soup_time = time.time()
        yield info_dict

        second_time = time.time()

        try:
            tab02 = driver.find_element_by_id('tab02')
            if tab02.get_attribute('aria-label') != '가격':
                raise NoSuchElementException()
        except NoSuchElementException as e:
            return iter([])
        driver.find_element_by_id('tab02').send_keys(Keys.ENTER)
        move_time = time.time()

        soup = BeautifulSoup(driver.page_source, 'html.parser')

        soup2_time = time.time()
        list_item = soup.find("div", {"class": "tab_detail_area"}).findAll("a", {"class": "list_item"})
        is_li_tag = False
        if len(list_item) < 1:
            list_item = soup.find("div", {"class": "tab_detail_area"}).findAll("li", {"class": "list_item"})
            is_li_tag = True
        list_time = time.time()
        menu_result = []
        for l in list_item:
            menu_item = {}
            for child in l.children:
                # child == thum_area or info_area
                if child.attrs['class'][0] == 'thumb_area':
                    # 썸네일이 있다면
                    menu_item['img'] = child.contents[0].contents[0].attrs['src']
                elif child.attrs['class'][0] == 'info_area':
                    # 정보 블록 가져오기
                    if len(child.contents) > 1:
                        menu_item['name'] = child.contents[1].text if is_li_tag else child.contents[0].contents[0]
                        menu_item['price'] = child.contents[0].text if is_li_tag else child.contents[1].text
                else:
                    pass
            menu_result.append(menu_item)

        final_time = time.time()
        print("\t반응 시간: ", driver_time - start)
        print("\tSoup1 시간: ", soup_time - driver_time)
        print("\tyield 시간: ", second_time - soup_time)
        print("\t반응2 시간: ", move_time - second_time)
        print("\tSoup2 시간: ", soup2_time - move_time)
        print("\tList 시간: ", list_time - soup2_time)
        print("\tfinal시간: ", final_time - list_time)
        print("\t>>>> 반응1 - 총 시간: ", soup_time - start)
        print("\t>>>> 반응2 - 총 시간: ", final_time - second_time)
        print("\t>>>> 총 시간: ", final_time - start)

        yield menu_result

    # 메뉴 가져오는부분
    def scrape_menu(self, id):
        start = time.time()
        response = requests.get("https://store.naver.com/restaurants/detail?id=" + str(id) + "&tab=menu#_tab")
        response_time = time.time()

        html = response.text
        soup = BeautifulSoup(html, 'html.parser')

        soup_time = time.time()

        # for name in soup.select('#content > div:nth-child(2) > div.bizinfo_area > div > div.list_item.list_item_menu > div > ul > li:nth-child(1) > div > div > div > span'):
        #     print(name.text)

        title = soup.findAll("div", {"class": "tit"})
        price = soup.findAll("div", {"class": "price"})

        price_time = time.time()
        result = ""
        for i, (t, p) in enumerate(zip(title, price)):
            # print(t.text, "\t", p.text)
            result += t.text + ' ' + p.text
            if len(title) - 1 != i:
                result += '|'

        final_time = time.time()

        # print("\t반응시간: ", response_time-start)
        # print("\tSoup시간: ", soup_time-response_time)
        # print("\tPrice시간: ", price_time-soup_time)
        # print("\tfinal시간: ", final_time-price_time)
        # print("\t>>>> 총 시간: ", final_time-start)

        return result

    # 평점, 데이터랩 단어 가져오는부분
    def scrape_main(self, id):
        response = requests.get("https://store.naver.com/restaurants/detail?entry=plt&id=" + str(id))
        html = response.text

        soup = BeautifulSoup(html, 'html.parser')
        try:
            score = \
                soup.select("#panel01 > div > div.sc_box.booking_review > div.raing_area > div.star_area > span.score")[
                    0]
            # rint("평점:",score.text)
        except:
            print("평점정보 없음")
        try:
            theme = soup.select("#panel01 > div > div.sc_box.datalab > div > div.theme_kwd_area > ul > li > span")[
                0]  # 분위기 가져오는곳, 숫자바꾸면 인기토픽,찾는목적도 가져올 수 있음

            # print("분위기:",theme.text)
        except:
            print("데이터랩 없음")
        category = soup.find("span", {"class": "category"})
        # print("카테고리:",category.text)

    # 사진정보 가져오는 부분
    # 2020.05.07 JH Completed.
    def scrape_photo(self, id):
        response = requests.get(
            "https://store.naver.com/restaurants/detail?entry=ple&id=" + str(id) + "&tab=photo&tabPage=0#_tab")
        html = response.text

        soup = BeautifulSoup(html, 'html.parser')
        poto = soup.findAll("img")
        for i in poto:
            # if 'type=m862_636' in i.get("src"):
            if i.get("src").find("type=m862_636") != -1:
                yield i.get("src").replace("&type=m862_636", "")


    # 리뷰, 평점 가져오는부분
    def scrape_review_score(self, id, page, only_score=False):
        add_score = 0;
        count = 0

        response = requests.get(
            "https://store.naver.com/restaurants/detail?entry=pll&id=" + str(
                id) + "&tab=receiptReview&tabPage=" + str(page))
        html = response.text
        soup = BeautifulSoup(html, 'html.parser')
        score = soup.findAll("span", {"class": "score"})
        reviewer_area = soup.findAll("div", {"class": "reviewer_area"}) if not only_score else [None for x in
                                                                                                range(len(score))]
        review_txt = soup.findAll("div", {"class": "review_txt"}) if not only_score else [None for x in
                                                                                          range(len(score))]
        total = soup.find("span", {"class": "total"}) if page == 0 else None

        review_array = []
        for i, j, k in zip(reviewer_area, review_txt, score):
            rating = float(k.text)
            if not only_score:
                children = i.contents[0].contents if len(i.contents) < 2 else i.contents[1].contents
                review_array.append(
                    {'name': children[0].text, 'date': children[1].text, 'rating': rating, 'text': j.text})
            add_score += rating
            count += 1
        avg_score = str(round(add_score / count, 1)) if count > 0 else 0

        if total is not None:
            total = total.text
        if only_score:
            return avg_score
        return review_array, avg_score, total

    def scrape_place_(self, lon, lat, radius):
        bounds_arr = GeoUtil.get_bounds(lon, lat, radius)
        url = "https://store.naver.com/restaurants/list?bounds=" + str(bounds_arr[0]) + "%3B" + str(
            bounds_arr[1]) + "%3B" + str(bounds_arr[2]) + "%3B" + str(bounds_arr[3]) + "&query=%EB%A7%9B%EC%A7%91"
        print(url)
        response = requests.get(url)
        html = response.text

        restaurants = []  # 분위기별 모든 식당이름을 저장 추후 id로 변경해야함
        r_ids = []  # id만 모은 배열

        start_str = 'window.PLACE_STATE=';
        end_str = '}</script>'
        parsing_source = html[html.find(start_str) + len(start_str):html.find(end_str) + 1]

        dict = json.loads(parsing_source)
        review = self.scrape_review_score

        # test용
        #from common_utils import APIKeyLoader
        """host_info = APIKeyLoader.load('host_setting.dll')

        if len(host_info) < 1:
            print('Please Check your host info. It should be placed at ../host_setting.dll')
        elif len(host_info) < 3:
            print("There's no DB Setting in host setting file. Please Check file again. path = ../host_setting.dll")

        import db_connector as db
        object = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])"""
        start_time = time.time()

        for i in dict['businesses'].keys():
            if i.startswith('[bounds:'):
                for j in dict['businesses'][i]['items']:
                    if j is not None and 'businessCategory' in j and j['businessCategory'] == 'restaurant':
                        # 중복체크 구문
                        query = "SELECT big_category FROM category_definition WHERE category = %s"
                        parameter = (j["category"], )
                        number = object.execute_one(query, parameter)

                        # 없는 카테고리인 경우 추가로 삽입한다.
                        if number is None:
                            query = "INSERT INTO category_definition (category, big_category) VALUES (%s, %s)"
                            parameter = (j['category'], 0)
                            object.execute(query, parameter)
                            object.commit()

                        res_code = j["id"]
                        res_name = j["name"]
                        res_category = j["category"]

                        ## DB Insert
                        query = "SELECT * FROM resturant as res WHERE res.res_code = %s"
                        parameter = (j["id"],)
                        result = object.execute_all(query, parameter)

                        # 신규라면
                        if len(result) == 0:
                            query = "INSERT INTO resturant (res_code, res_name, res_category) VALUES (%s, %s, %s)"
                            parameter = (res_code, res_name, str(number["big_category"] if number is not None else 0))
                            print("%s %s %s" % (res_code, res_name, res_category))
                            object.execute(query, parameter)
                            object.commit()

                        restaurants.append({
                            "id": j["id"],
                            "name": j["name"],
                            "category": j['category'],
                            'lon': j['x'], 'lat': j['y'],
                            # 'rating' : review(j['id'], 0, True) # 추후 성능 개선 후 주석 해제
                            # 평점을 tab_main에 있는 평점을 가져와도 될 것 같기도 함.
                        })
                        # r_ids.append(j['id'])




        end_time = time.time()

        print("adjust : %d seconds" % (end_time - start_time))
        # 평점 및 대표메뉴 조회
        # DB에 있는 식당만 결과 적용
        #start_time = time.time()

        # for id in r_ids:
        #     pass
        # pool = multiprocessing.Pool()
        # pool.map(self.scrape_menu, r_ids)
        # with multiprocessing.Pool() as p:
        #     brief_things = p.map(scrape_alone, r_ids)
        return restaurants

    def func(self, arr):
        print(arr)
    def get_point(self, num):
        _sum = 0.0
        tab_page_num = 0
        end_point = 0
        total_num = 0
        while True:
            if end_point is not 0 and tab_page_num is end_point:
                score = _sum / total_num
                break
            response = requests.get("https://store.naver.com/restaurants/detail?entry=plt&id=%s&tab=receiptReview&tabPage=%s" % (str(num), str(tab_page_num)))
            time.sleep(1.5)
            html = response.text
            soup = BeautifulSoup(html, 'html.parser')
            if tab_page_num is 0:
                total_num = soup.select('h3 > span.count')
            ## 오류 체크

            #print(total_num)


            if total_num is 0 or (type(total_num) is not int and len(total_num) is 0):
                score = 0
                break
            else:
                if end_point is 0:
                    import re
                    total_num = int(re.findall("\d+", str(total_num))[0])

                    # 한페이지당 최대 10개 댓글
                    MAX_PAGE_COMMENT = 10
                    end_point = (total_num // MAX_PAGE_COMMENT) \
                        if (total_num / MAX_PAGE_COMMENT * MAX_PAGE_COMMENT) is total_num \
                        else (total_num // MAX_PAGE_COMMENT) + 1

                result = soup.find_all("span", class_=['score'])
                for row in result:
                    row_score = int(re.findall("\d+", str(row))[0])
                    _sum += row_score
                #print(result)
                tab_page_num += 1

        return score

    def get_menu(self, num):
        response = requests.get("https://store.naver.com/restaurants/detail?entry=plt&id=%s&tab=receiptReview&tabPage=0" % str(num))
        time.sleep(1.5)
        html = response.text
        soup = BeautifulSoup(html, 'html.parser')
        result_name = soup.select('div.menu > span.name')
        result_price = soup.select('div.list_menu_inner > em.price')
        import re
        if len(result_name) is 0:
            return ["없음", "없음"]
        else:
            name = re.sub('<.+?>', '', str(result_name[0]), 0, re.I|re.S)
            price = re.sub('<.+?>', '', str(result_price[0]), 0, re.I|re.S)
            return_arr = [name, price]

            """for row in result_name:
                if len(return_arr) > 0:
                    break
                row = re.sub('<.+?>', '', str(row), 0, re.I|re.S)
                return_arr.append(row, result_price[0])"""
            return return_arr

    def thread_func(self, num):
        print(num)
        point = self.get_point(num)
        menu = self.get_menu(num)
        print("점수 : %s, 대표메뉴 : %s, 메뉴가격 : %s" % (point, menu[0], menu[1]))

        ## update -> commit
        query = "UPDATE resturant SET res_grade = %s, res_menu = %s, res_price = %s where res_code = %s"
        parameter = (point, menu[0], menu[1], num)
        object.execute(query, parameter)
        object.commit()
        time.sleep(1)

    def process_thread(self, arr):
        import concurrent.futures as cf
        executor = cf.ThreadPoolExecutor(max_workers=len(arr))
        for i in range(len(arr)):
            executor.submit(self.thread_func(arr[i]))

    def zzim(self, usr_email):
        #db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])

        query = 'SELECT res_category, COUNT(*) as cnt FROM (SELECT * FROM pickupres WHERE usr_code = (SELECT usr_code FROM user WHERE usr_email = %s) ORDER BY pup_regdate DESC, pup_regtime DESC LIMIT 20) AS p JOIN resturant AS r ON p.res_code = r.res_code group by res_category ORDER BY cnt DESC'
        #query = 'SELECT res_category, COUNT(*) FROM (SELECT * FROM pickupres WHERE usr_code = (SELECT usr_code FROM user WHERE usr_email = "1") ORDER BY pup_regdate DESC, pup_regtime DESC LIMIT 10) AS p JOIN resturant AS r ON p.res_code = r.res_code group by res_category;'
        parameter = (usr_email,)
        result = object.execute_all(query, parameter)

        print(result)
        recommends = []
        if len(result) <= 4:
            for i in range(len(result)):
                recommends.append(result[i]["res_category"])
        else:
            for i in range(0, 4):
                recommends.append(result[i]["res_category"])
        print(recommends)

        return recommends


    def enter_naver(self, lon, lat, radius, email):
        bounds_arr = GeoUtil.get_bounds(lon, lat, radius)
        url = "https://store.naver.com/restaurants/list?bounds=" + str(bounds_arr[0]) + "%3B" + str(
            bounds_arr[1]) + "%3B" + str(bounds_arr[2]) + "%3B" + str(bounds_arr[3]) + "&query=%EB%A7%9B%EC%A7%91"
        # print(url)
        response = requests.get(url)
        html = response.text

        restaurants = []  # 분위기별 모든 식당이름을 저장 추후 id로 변경해야함
        r_ids = []  # id만 모은 배열

        start_str = 'window.PLACE_STATE='
        end_str = '}</script>'
        parsing_source = html[html.find(start_str) + len(start_str):html.find(end_str) + 1]

        dict = json.loads(parsing_source)
        review = self.scrape_review_score

        # test용
        # from common_utils import APIKeyLoader
        start_time = time.time()
        find_arr = []
        for i in dict['businesses'].keys():
            if i.startswith('[bounds:'):
                for j in dict['businesses'][i]['items']:
                    if j is not None and 'businessCategory' in j and j['businessCategory'] == 'restaurant':
                        res_code = j["id"]
                        res_name = j["name"]
                        res_category = j["category"]
                        res_lon = j['x']
                        res_lat = j['y']
                        ## DB Insert
                        query = "SELECT * FROM resturant as res WHERE res.res_code = %s"
                        parameter = (j["id"],)
                        result = object.execute_all(query, parameter)

                        # 신규라면
                        if len(result) == 0:
                            query = "INSERT INTO resturant (res_code, res_name, res_category, res_lon, res_lat) VALUES (%s, %s, %s, %s, %s)"
                            parameter = (res_code, res_name, res_category, res_lon, res_lat)
                            # print("%s %s %s" % (res_code, res_name, res_category))
                            object.execute(query, parameter)
                            object.commit()
                            ########################### multithread
                            find_arr.append(j["id"])

                        restaurants.append({
                            "id": j["id"],
                            "name": j["name"],
                            "category": j['category'],
                            'lon': j['x'], 'lat': j['y'],
                            # 'rating' : review(j['id'], 0, True) # 추후 성능 개선 후 주석 해제
                            # 평점을 tab_main에 있는 평점을 가져와도 될 것 같기도 함.
                        })
                        # r_ids.append(j['id'])
        end_time = time.time()

        print("adjust : %d seconds" % (end_time - start_time))

        if len(find_arr) > 0:
            self.process_thread(find_arr)
        #return restaurants, find_arr

    def enter_naver2(self, lon, lat, radius, target_category, index, origin_arr):
        print(origin_arr)
        bounds_arr = GeoUtil.get_bounds(lon, lat, radius)

        #convert_str = target_category.encode('UTF-8')
        url = "https://store.naver.com/restaurants/list?bounds=" + str(bounds_arr[0]) + "%3B" + str(
            bounds_arr[1]) + "%3B" + str(bounds_arr[2]) + "%3B" + str(bounds_arr[3]) + "&query=%s" % target_category

        #print("after : " + target_category.encode('UTF-8'))
        print(url)
        response = requests.get(url)
        time.sleep(1.5)
        html = response.text

        restaurants = []  # 분위기별 모든 식당이름을 저장 추후 id로 변경해야함
        r_ids = []  # id만 모은 배열

        start_str = 'window.PLACE_STATE='
        end_str = '}</script>'
        parsing_source = html[html.find(start_str) + len(start_str):html.find(end_str) + 1]

        dict = json.loads(parsing_source)
        review = self.scrape_review_score

        # test용
        # from common_utils import APIKeyLoader
        start_time = time.time()
        find_arr = []
        for i in dict['businesses'].keys():
            if i.startswith('[bounds:'):
                for j in dict['businesses'][i]['items']:
                    if j is not None and 'businessCategory' in j and j['businessCategory'] == 'restaurant':
                        res_code = j["id"]
                        res_name = j["name"]
                        res_category = j["category"]
                        res_lon = j['x']
                        res_lat = j['y']

                        ## id가 중복되는 경우
                        if res_code in origin_arr:
                            continue
                        find_arr.append(j["id"])
                        ## DB Insert
                        query = "SELECT * FROM resturant as res WHERE res.res_code = %s"
                        parameter = (j["id"],)
                        result = object.execute_all(query, parameter)

                        # 신규라면
                        if len(result) == 0:
                            query = "INSERT INTO resturant (res_code, res_name, res_category, res_lon, res_lat) VALUES (%s, %s, %s, %s, %s)"
                            parameter = (res_code, res_name, res_category, res_lon, res_lat)
                            # print("%s %s %s" % (res_code, res_name, res_category))
                            object.execute(query, parameter)
                            object.commit()
                            ########################### multithread
                            #find_arr.append(j["id"])

                        restaurants.append({
                            "id": j["id"],
                            "name": j["name"],
                            "category": j['category'],
                            'lon': j['x'], 'lat': j['y'],
                            'rank':index
                            # 'rating' : review(j['id'], 0, True) # 추후 성능 개선 후 주석 해제
                            # 평점을 tab_main에 있는 평점을 가져와도 될 것 같기도 함.
                        })
                        # r_ids.append(j['id'])
        end_time = time.time()

        print("adjust : %d seconds" % (end_time - start_time))

        if index is 0:
            res_ret = restaurants[0:7] if len(restaurants) >= 7 else restaurants
            return res_ret, find_arr
        else:
            res_ret = restaurants[0:4] if len(restaurants) >= 4 else restaurants
            return res_ret, find_arr

    def enter_naver3(self, lon, lat, radius, find_num, origin_arr):
        bounds_arr = GeoUtil.get_bounds(lon, lat, radius)

        url = "https://store.naver.com/restaurants/list?bounds=" + str(bounds_arr[0]) + "%3B" + str(
            bounds_arr[1]) + "%3B" + str(bounds_arr[2]) + "%3B" + str(
            bounds_arr[3]) + "&query=%EB%A7%9B%EC%A7%91&sortingOrder=reviewCount"

        print(url)
        response = requests.get(url)
        time.sleep(1.5)
        html = response.text

        restaurants = []  # 분위기별 모든 식당이름을 저장 추후 id로 변경해야함
        r_ids = []  # id만 모은 배열

        start_str = 'window.PLACE_STATE='
        end_str = '}</script>'
        parsing_source = html[html.find(start_str) + len(start_str):html.find(end_str) + 1]

        dict = json.loads(parsing_source)
        review = self.scrape_review_score

        # test용
        # from common_utils import APIKeyLoader
        start_time = time.time()
        find_arr = []

        add_num = 0

        for i in dict['businesses'].keys():
            if i.startswith('[bounds:'):
                for j in dict['businesses'][i]['items']:
                    if j is not None and 'businessCategory' in j and j['businessCategory'] == 'restaurant':
                        res_code = j["id"]
                        res_name = j["name"]
                        res_category = j["category"]
                        res_lon = j['x']
                        res_lat = j['y']

                        ## 개수가 다 차는 경우
                        if add_num == find_num:
                            break

                        ## id가 중복되는 경우
                        if res_code in origin_arr:
                            continue
                        find_arr.append(j["id"])

                        add_num += 1
                        ## DB Insert
                        query = "SELECT * FROM resturant as res WHERE res.res_code = %s"
                        parameter = (j["id"],)
                        result = object.execute_all(query, parameter)

                        # 신규라면
                        if len(result) == 0:
                            query = "INSERT INTO resturant (res_code, res_name, res_category, res_lon, res_lat) VALUES (%s, %s, %s, %s, %s)"
                            parameter = (res_code, res_name, res_category, res_lon, res_lat)
                            # print("%s %s %s" % (res_code, res_name, res_category))
                            object.execute(query, parameter)
                            object.commit()

                        restaurants.append({
                            "id": j["id"],
                            "name": j["name"],
                            "category": j['category'],
                            'lon': j['x'], 'lat': j['y'],
                            # 'rating' : review(j['id'], 0, True) # 추후 성능 개선 후 주석 해제
                            # 평점을 tab_main에 있는 평점을 가져와도 될 것 같기도 함.
                        })
                        # r_ids.append(j['id'])
        end_time = time.time()

        print("adjust : %d seconds" % (end_time - start_time))

        return restaurants, find_arr

        """
        if index is 0:
            res_ret = restaurants[0:7] if len(restaurants) >= 7 else restaurants
            return res_ret, find_arr
        else:
            res_ret = restaurants[0:7] if len(restaurants) >= 4 else restaurants
            return res_ret, find_arr
        """


    def scrape_place(self, lon, lat, radius, email):
        #restaurants, find_arr = self.enter_naver(lon, lat, radius, email)

        query = "SELECT COUNT(*) as cnt FROM pickupres as p JOIN user as u ON p.usr_code = u.usr_code WHERE u.usr_email = %s"
        parameter = (email, )
        zzim_num = object.execute_all(query, parameter)

        restaurants = []
        find_arr = []

        print(zzim_num[0]['cnt'])
        if zzim_num[0]['cnt'] >= 20:
            result = self.zzim(email)
            print(result)
            for index in range(len(result)):
                target_category = result[index]
                print("index : %d" % index)
                target_restaurants, target_arr = self.enter_naver2(lon, lat, radius, target_category, index, find_arr)
                restaurants.extend(target_restaurants)
                find_arr.extend(target_arr)
            #print(restaurants)
            #print(find_arr)

            if len(restaurants) < 20:
                print(len(restaurants))
                print(restaurants)
                tg_restaurants, tg_arr = self.enter_naver3(lon, lat, radius, 20 - len(restaurants), find_arr)
                restaurants.extend(tg_restaurants)
                find_arr.extend(tg_arr)
                print("빵")
        else:
            tg_restaurants, tg_arr = self.enter_naver3(lon, lat, radius, 20, find_arr)
            restaurants.extend(tg_restaurants)
            find_arr.extend(tg_arr)
            print("빵")


        #if len(find_arr) > 0:
        #    self.process_thread(find_arr)


        print(restaurants)

        # 평점 및 대표메뉴 조회
        # DB에 있는 식당만 결과 적용
        #start_time = time.time()

        # for id in r_ids:
        #     pass
        # pool = multiprocessing.Pool()
        # pool.map(self.scrape_menu, r_ids)
        # with multiprocessing.Pool() as p:
        #     brief_things = p.map(scrape_alone, r_ids)
        return restaurants
