import pymysql


class DBConnector:
    def __init__(self, host, user, password, db):
        self.db = pymysql.connect(host=host, user=user, password=password, db=db, charset='utf8')
        self.cursor = self.db.cursor(pymysql.cursors.DictCursor)

    def execute(self, query, args={}):
        self.cursor.execute(query, args)

    def execute_one(self, query, args={}):
        self.cursor.execute(query, args)
        row = self.cursor.fetchone()
        return row

    def execute_all(self, query, args={}):
        self.cursor.execute(query, args)
        row = self.cursor.fetchall()
        return row

    def commit(self):
        self.db.autocommit(True)

    def rollback(self):
        self.db.rollback()

    def close(self):
        self.db.close()