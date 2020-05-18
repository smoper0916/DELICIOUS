import json
import os, requests, ssl

from flask import Flask, jsonify, request, send_file, abort, render_template, make_response, Response
from common_utils import * # 공통 유틸 Import
from naver_scraper import NaverScraper  # 네이버 크롤러
import time
import db_connector as db

host_info = APIKeyLoader.load('host_setting.dll')
if len(host_info) < 1:
    print('Please Check your host info. It should be placed at ../host_setting.dll')
elif len(host_info) < 3:
    print("There's no DB Setting in host setting file. Please Check file again. path = ../host_setting.dll")

app = Flask(__name__)   # 웹서버 초기화(세션 초기화 한것이 app에 들어가있음)
context = ssl.SSLContext(ssl.PROTOCOL_TLS)  # sslcontext 초기화(ssl쓰기 위해 반드시 초기해야함(인증하기위함))
cert = 'ssl/cert.pem'
pkey = 'ssl/privkey.pem'
# cipher = 'DHE-RSA-AES128-SHA:DHE-RSA-AES256-SHA:ECDHE-ECDSA-AES128-GCM-SHA256'

context.load_cert_chain(certfile=cert, keyfile=pkey, password='')   # 인증서 로드
root_path = '/deli/v1' # 기본 경로
n_scraper = NaverScraper()

db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])


# 상대경로의 Full Path를 넘겨주는 함수
def get_fullpath(rpath):
    return root_path + rpath


@app.errorhandler(500)
def resource_not_found(e):
    return jsonify(error=e.code, code=e.description), 500


@app.errorhandler(400)
@app.errorhandler(401)
@app.errorhandler(403)
@app.errorhandler(404)
@app.errorhandler(405)
def resource_not_found(e):
    return jsonify(error=e.code, code=e.description), 400


@app.route(get_fullpath('/restaurants/near'), methods=['GET'])
def browse_restaurants():
    '''
    BROWSE RESTAURANTS API
    GPS 좌표 기반으로 주변의 식당 정보 제공
    x: 경도 (그리니치 천문대 기준 동경 130도 정도)
    y: 위도 (적도 기준 북위 35~36도 정도)
    radius: 반경(m)
    :return: JSON 형태의 식당 리스트
    '''

    result_dict = {}

    # POST 데이터를 받기 위해 request.form을 사용
    # string으로 전송하는 값은 float으로 변환.
    lon = float(request.args.get('lon'))
    lat = float(request.args.get('lat'))
    radius = int(request.args.get('radius'))

    # 네이버 플레이스 파싱
    start = time.time()
    result_dict['restaurants'] = n_scraper.scrape_place(lon, lat, radius)

    print("속도 : ", time.time() - start)

    return jsonify(result_dict) #string 타입을 json형식으로 볼수있는 함수


@app.route(get_fullpath('/restaurant/<id>/menu'), methods=['GET'])
def get_menu(id):
    result_dict = {}
    for name, item in zip(['info', 'menu'], n_scraper.scrape_information(id)):
        result_dict[name] = item

    return jsonify(result_dict)


@app.route(get_fullpath('/restaurant/<id>/reviews'), methods=['GET'])
def get_reviews(id):
    result_dict = {}
    page = request.args.get('page')

    result_dict['records'], result_dict['avg'], pages = n_scraper.scrape_review_score(id, 0 if page is None else int(page))
    if pages is not None:
        result_dict['pages'] = pages
    return jsonify(result_dict)


@app.route(get_fullpath('/restaurant/<id>/photo'), methods=['GET'])
def get_photo(id):
    result_dict = {}
    result_dict['urls'] = [x for x in n_scraper.scrape_photo(id)]
    return jsonify(result_dict)


@app.route(get_fullpath('/<usr_email>'), methods=['GET'])
def check_user_email(usr_email):
    #db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])
    query = "SELECT * FROM user WHERE usr_email = %s"
    result = db_conn.execute_all(query, usr_email)
    #result_dict = {}
    #db_conn.close()
    if len(result) is not 0:
        abort(400, "existedAccount")

    return json.dumps({'code':'success'}), 200, {'ContentType':'application/json'}


@app.route(get_fullpath('/<usr_email>/profile'), methods=['GET'])
def get_user(usr_email):
    #db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])
    query = "SELECT * FROM user WHERE usr_email = %s"
    result = db_conn.execute_all(query, usr_email)
    result_dict = {}

    if len(result) is 0:
        abort(400, "You can't approach this personal information")
    result = result[0]
    db_conn.commit()
    #db_conn.close()
    return jsonify(user={'code':result['usr_code'],
                         'name':result['usr_name'],
                         'age':result['usr_age'],
                         'sex':result['usr_sex']})


@app.route(get_fullpath('/<usr_email>/profile'), methods=['POST'])
def create_user(usr_email):
    #db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])
    ####### 중복 확인해서 미리 거르는 코드
    input_email = usr_email
    duplicate_check_query = "SELECT * FROM user WHERE usr_email = %s"

    if len(db_conn.execute_all(duplicate_check_query, input_email)) > 0:
        abort(400, "existedAccount")
    ####### 중복 확인해서 미리 거르는 코드

    ##### if it doesn't have token
    # 1. 이메일로 가입하는 경우
    if 'token' not in request.form:
        query = "INSERT INTO user (usr_email, usr_name, usr_sex, usr_age, usr_pw) VALUES (%s, %s, %s, %s, %s)"
        usr_name = request.form['name']
        usr_sex = request.form['sex']
        usr_age = int(request.form['age'])
        usr_pw = request.form['password']

        parameter = (usr_email, usr_name, usr_sex, usr_age, usr_pw)
        db_conn.execute(query, parameter)
        db_conn.commit()

    ##### if it has token
    # 2. 카카오톡 or 네이버 아이디로 가입하는 경우
    else :
        ### usr_name, usr_sex, usr_age 보류
        # 일단은 다 넣었음, 보류한거 뺄수도 있음
        query = "INSERT INTO user (usr_email, usr_name, usr_sex, usr_token, usr_age, usr_pw) VALUES (%s, %s, %s, %s, %s, %s)"
        usr_name = request.form['name']
        usr_sex = request.form['sex']
        usr_token = request.form['token']
        usr_age = int(request.form['age'])
        usr_pw = request.form['password']

        parameter = (usr_email, usr_name, usr_sex, usr_token, usr_age, usr_pw)
        db_conn.execute(query, parameter)
        db_conn.commit()

    #db_conn.close()
    # 성공 신호 200 보내기
    return json.dumps({'code':'success'}), 200, {'ContentType':'application/json'}


@app.route(get_fullpath('/<usr_email>/profile'), methods=['PUT'])
def update_user(usr_email):
    #db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])
    param_json = json.loads(request.get_data().decode('ascii'))
    query = "UPDATE user SET usr_pw = %s WHERE usr_email = %s"
    usr_pw = param_json['new_password']

    parameter = (usr_pw, usr_email)
    db_conn.execute_one(query, parameter)
    db_conn.commit()
    #db_conn.close()
    return json.dumps({'code':'success'}), 200, {'ContentType':'application/json'}


@app.route(get_fullpath('/<usr_email>'), methods=['DELETE'])
def delete_user(usr_email):
    #db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])
    query = "DELETE FROM user WHERE usr_email = %s"
    db_conn.execute(query, usr_email)
    db_conn.commit() # 회원 탈퇴 commit
    #db_conn.close()
    return json.dumps({'code':'success'}), 200, {'ContentType':'application/json'}


@app.route(get_fullpath('/oauth'), methods=['GET'])
def kakao_auth():
    code = request.args.get('code')
    return render_template('kakao_response.html')


@app.route(get_fullpath('/user/auth'), methods=['POST'])
def user_auth():
    email = request.form["id"]
    password = request.form["password"]
    #db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])

    #email = request.args.get("id")
    #password = request.args.get("password")
    #db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])
    print("input_email : " + email + ", input_password : " + password + "\n")
    query_account_exist = "SELECT * FROM user WHERE usr_email = %s"
    _parameter = (email, )
    _result = db_conn.execute_all(query_account_exist, _parameter)

    ## 없는 아이디 처리
    if len(_result) is 0:
        #db_conn.close()
        return json.dumps({'code':'unexpected'}), 500, {'ContentType':'application/json'}
    else:
        query_auth_pw = "SELECT * FROM user WHERE usr_email = %s AND usr_pw = %s"
        parameter = (email, password)
        result = db_conn.execute_all(query_auth_pw, parameter)
        ### 비밀번호 오류
        if len(result) is 0:
            #db_conn.close()
            return json.dumps({'code': 'notMatch'}), 400, {'ContentType': 'application/json'}
        ### 정상 작동
        else:
            #db_conn.close()
            return json.dumps({'code': 'success'}), 200, {'ContentType': 'application/json'}

@app.route(get_fullpath('/<usr_email>/check'), methods=['GET'])
def duplicate_check_user(usr_email):
    #db_conn = db.DBConnector(host=host_info[2], user=host_info[3], password=host_info[4], db=host_info[5])
    query = "SELECT * FROM user WHERE usr_email = %s"
    parameter = (usr_email,)
    result = db_conn.execute_all(query, parameter)
    #db_conn.close()
    if len(result) > 0:
        return json.dumps({'code': 'duplicated'}), 400, {'ContentType': 'application/json'}
    else:
        return json.dumps({'code': 'success'}), 200, {'ContentType': 'application/json'}
'''
    limit remote addr : 
    모든 요청 처리 전 미리 host를 확인해서 도메인이 아닌 IP로의 접근을 원천 차단함.
'''
@app.before_request #요청 되기전에 실행되는 구문
def limit_remote_addr(): #IP주소로 들어오는 외국의 못된 친구들을 막기 위함
    if request.host == (host_info[0]+':'+host_info[1]):
        abort(403)  # Forbidden


if __name__ == '__main__':
    app.config['JSON_AS_ASCII'] = False #한글을 깨지는거 방지(utf-8)
    app.run(host='0.0.0.0', port=host_info[1], ssl_context=context) #외부 오픈(0.0.0.0) #port = 80(http(default)), 433(https(ssl))
    #db_conn.close()