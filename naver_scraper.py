from bs4 import BeautifulSoup
import requests
from flask import json
# import multiprocessing
from common_utils import GeoUtil
import time
from selenium import webdriver


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
        desc_area = soup.find("div", {"class":"list_item list_item_desc"})
        if desc_area is not None:
            btn_more = desc_area.find("a", {"class":"btn_more", "aria-label":"펼쳐보기"})
            if btn_more is not None:
                driver.find_element_by_xpath('//div/a[@aria-label="펼쳐보기"]').click()
                soup = BeautifulSoup(driver.page_source, 'html.parser')
        bizinfo_area = soup.find("div", {"class": "bizinfo_area"})  # 모든 기본 정보가 담긴 div
        # biz_message_area = soup.find("div", {"class": "default_info_area biz_message_area"})  # N페이 예약 혜택

        '''
            biztime : 영업시간 / telephone : 전화번호 / addr : 도로명주소 / homepage : 홈페이지 / convenience : 편의시설
            tv_history : TV 출연내역 / desc : 식당 설명
        '''
        for key_name, (tag_name, tag_class) in zip(["biztime", "telephone", "addr", "convenience", "tv_history", "desc"],
                           [("div", "biztime_row"), ("div", "list_item list_item_biztel"), ("span", "addr"),
                            ("div", "convenience"), ("div", "list_item list_item_tv"),
                            ("div", "list_item list_item_desc"), ]):
            parsed = bizinfo_area.find(tag_name, {"class": tag_class}) # Business Time
            if parsed is not None:
                info_dict[key_name] = parsed.text

        # Not None Value 채워넣기
        # for key_name, value in [('nlink', nlink), ('npay_msg', biz_message_area)]:
        #     if value is not None:
        #         info_dict[key_name] = value.attrs['href'] if key_name == 'nlink' else value.text
        soup_time = time.time()
        yield info_dict

        second_time = time.time()
        driver.find_element_by_id('tab02').click()
        move_time = time.time()

        soup = BeautifulSoup(driver.page_source, 'html.parser')

        soup2_time = time.time()
        list_item = soup.find("div", {"class": "tab_detail_area"}).findAll("a", {"class": "list_item"})

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
                    menu_item['name'] = child.contents[0].contents[0]
                    menu_item['price'] = child.contents[1].text
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
            soup.select("#panel01 > div > div.sc_box.booking_review > div.raing_area > div.star_area > span.score")[0]
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
        add_score = 0; count = 0

        response = requests.get(
            "https://store.naver.com/restaurants/detail?entry=pll&id=" + str(
            id) + "&tab=receiptReview&tabPage=" + str(page))
        html = response.text
        soup = BeautifulSoup(html, 'html.parser')
        score = soup.findAll("span", {"class": "score"})
        reviewer_area = soup.findAll("div", {"class": "reviewer_area"}) if not only_score else [None for x in range(len(score))]
        review_txt = soup.findAll("div", {"class": "review_txt"}) if not only_score else [None for x in range(len(score))]
        total = soup.find("span", {"class": "total"}) if page == 0 else None

        review_array = []
        for i, j, k in zip(reviewer_area, review_txt, score):
            rating = float(k.text)
            if not only_score:
                children = i.contents[0].contents if len(i.contents) < 2 else i.contents[1].contents
                review_array.append({'name': children[0].text, 'date': children[1].text, 'rating': rating, 'text': j.text})
            add_score += rating
            count += 1
        avg_score = str(round(add_score / count, 1)) if count > 0 else 0

        if total is not None:
            total = total.text
        if only_score:
            return avg_score
        return review_array, avg_score, total

    def scrape_place(self, lon, lat, radius):
        bounds_arr = GeoUtil.get_bounds(lon, lat, radius)
        url = "https://store.naver.com/restaurants/list?bounds=" + str(bounds_arr[0]) + "%3B" + str(
            bounds_arr[1]) + "%3B" + str(bounds_arr[2]) + "%3B" + str(bounds_arr[3]) + "&query=%EB%A7%9B%EC%A7%91"
        print(url)
        response = requests.get(url)
        html = response.text

        restaurants = []  # 분위기별 모든 식당이름을 저장 추후 id로 변경해야함
        r_ids = [] # id만 모은 배열

        start_str = 'window.PLACE_STATE=';
        end_str = '}</script>'
        parsing_source = html[html.find(start_str) + len(start_str):html.find(end_str) + 1]

        dict = json.loads(parsing_source)
        review = self.scrape_review_score

        for i in dict['businesses'].keys():
            if i.startswith('[bounds:'):
                for j in dict['businesses'][i]['items']:
                    if j is not None and 'businessCategory' in j and j['businessCategory'] == 'restaurant':
                        restaurants.append({
                            "id" : j["id"],
                            "name" : j["name"],
                            "category" : j['category'],
                            'lon' : j['x'], 'lat': j['y'],
                            # 'rating' : review(j['id'], 0, True) # 추후 성능 개선 후 주석 해제
                            # 평점을 tab_main에 있는 평점을 가져와도 될 것 같기도 함.
                        })
                        # r_ids.append(j['id'])

        # 평점 및 대표메뉴 조회
        # DB에 있는 식당만 결과 적용
        start_time = time.time()

        # for id in r_ids:
        #     pass
        # pool = multiprocessing.Pool()
        # pool.map(self.scrape_menu, r_ids)
        # with multiprocessing.Pool() as p:
        #     brief_things = p.map(scrape_alone, r_ids)
        return restaurants