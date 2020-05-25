import requests, json

from common_utils import APIKeyLoader


def web_request(method_name, url, dict_data, is_urlencoded=True):
    """Web GET or POST request를 호출 후 그 결과를 dict형으로 반환 """
    method_name = method_name.upper()  # 메소드이름을 대문자로 바꾼다
    if method_name not in ('GET', 'POST'):
        raise Exception('method_name is GET or POST plz...')

    if method_name == 'GET':  # GET방식인 경우
        response = requests.get(url=url, params=dict_data)
    elif method_name == 'POST':  # POST방식인 경우
        if is_urlencoded is True:
            response = requests.post(url=url, data=dict_data,
                                     headers={'Content-Type': 'application/x-www-form-urlencoded'})
        else:
            response = requests.post(url=url, data=json.dumps(dict_data), headers={'Content-Type': 'application/json'})

    dict_meta = {'status_code': response.status_code, 'ok': response.ok, 'encoding': response.encoding,
                 'Content-Type': response.headers['Content-Type']}
    if 'json' in str(response.headers['Content-Type']):  # JSON 형태인 경우
        return {**dict_meta, **response.json()}
    else:  # 문자열 형태인 경우
        return {**dict_meta, **{'text': response.text}}

# initial parameter
# start_pos : Busan Fire Headquarter
# end_pos : Yeonsan Library
startX = "128.422101"
startY = "36.138313"
endX = "128.418121"
endY = "36.137834"
reqCoordType = "WGS84GEO"
resCoordType = "WGS84GEO"
startName = "금오공대 정문"
endName = "안드로메다"

tmap_project_keys = APIKeyLoader.load('tmap_project_key.dll')

data = {
    "appKey" : tmap_project_keys[0],
    "startX": startX,
    "startY": startY,
    "endX": endX,
    "endY": endY,
    "reqCoordType": reqCoordType,
    "resCoordType": resCoordType,
    'startName' : startName,
    'endName' : endName
}

url = "https://apis.openapi.sk.com/tmap/routes/pedestrian?version=1&format=json&callback=result"
response = requests.request(method='POST', url=url, data=data)
#response = web_request(method_name='POST', url=url, dict_data=data)

if response.status_code == 200:

    print(type(response))
    json_data = response.json()
    #print(type(json_data))
    print(json.dumps(json_data, indent=4, ensure_ascii=False))

    point_arr = []
    for i in json_data["features"]:
        geo = i["geometry"]
        if i["geometry"]["type"] == "LineString":
            point_arr.extend(geo["coordinates"])

    print(point_arr)
    """
    json_data = json.dumps(response)
    final_data = json.loads(json_data)
    print(type(response))
    print(type(json_data))
    print(type(final_data))
    """
else:
    print("ERROR")