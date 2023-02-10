# -*- coding: utf-8 -*-
"""
    MiniTwit Tests
    ~~~~~~~~~~~~~~

    Tests the MiniTwit application.

    :copyright: (c) 2010 by Armin Ronacher.
    :license: BSD, see LICENSE for more details.
"""
import requests
import minitwit
import unittest
import tempfile
import json


class MiniTwitTestCase(unittest.TestCase):

    def setUp(self):
        """Before each test, set up a blank database"""
        self.db = tempfile.NamedTemporaryFile()
        self.app = minitwit.app.test_client()
        self.url = 'http://localhost:5235'
        self.decoding = "utf-8"
        minitwit.DATABASE = self.db.name
        minitwit.init_db()

    # helper functions

    def register(self, username, password, password2=None, email=None):
        """Helper function to register a user"""
        if password2 is None:
            password2 = password
        if email is None:
            email = username + '@example.com'
        resp = requests.post('http://localhost:5235/register', json = {
            'username':     username,
            'password':     password,
            'password2':    password2,
            'email':        email,
        })
        return resp


    def login(self, username, password):
        """Helper function to login"""
        return requests.post(self.url + "/login", json = {
            'username': username,
            'password': password
        })

    def register_and_login(self, username, password):
        """Registers and logs in in one go"""
        self.register(username, password)
        return self.login(username, password)

    def logout(self):
        """Helper function to logout"""
        return requests.get(self.url + "/logout")

    def add_message(self, text):
        """Records a message"""
        rv = requests.post(self.url + "/add_message", json = {'text': text})
        #rv = self.app.post('/add_message', data={'text': text},
        #                            follow_redirects=True)
        if text:
            self.assertEqual('Your message was recorded', rv.content.decode(self.decoding))
        return rv
    
    # get content from rv
    def getContent(self,rv):
        return rv.content.decode(self.decoding)

    # testing functions
    def test_simple_register(self):
        """homemade simple test failed"""
        rv = self.register('user1', 'default')
        self.assertEqual('You were successfully registered and can login now', self.getContent(rv))

    def test_register(self):
        """Make sure registering works"""
        rv = self.register('user1', 'default')
        self.assertEqual('You were successfully registered and can login now', rv.content.decode(self.decoding))
        rv = self.register('user1', 'default')
        self.assertEqual('The username is already taken', rv.content.decode(self.decoding))
        rv = self.register('', 'default')
        self.assertEqual('You have to enter a username', rv.content.decode(self.decoding))
        rv = self.register('meh', '')
        self.assertEqual('You have to enter a password' == (rv.content.decode(self.decoding)))
        rv = self.register('meh', 'x', 'y')
        self.assertEqual('The two passwords do not match' == rv.content.decode(self.decoding))
        rv = self.register('meh', 'foo', email='broken')
        self.assertEqual('You have to enter a valid email address' == (rv.content.decode(self.decoding)))

    def test_login_logout(self):
        """Make sure logging in and logging out works"""
        rv = self.register_and_login('user1', 'default')
        self.assertEqual('You were logged in', self.getContent(rv))
        rv = self.logout()
        self.assertEqual('You were logged out', self.getContent(rv))
        rv = self.login('user1', 'wrongpassword')
        self.assertEqual('Invalid password', self.getContent(rv))
        rv = self.login('user2', 'wrongpassword')
        self.assertEqual('Invalid username', self.getContent(rv))

    def test_message_recording(self):
        """Check if adding messages works"""
        self.register_and_login('foo', 'default')
        self.add_message('test message 1')
        self.add_message('<test message 2>')
        rv = self.getContent(requests.get(self.url + "/"))
        # in can find substrings, maybe
        self.assertTrue('test message 1' in rv)
        self.assertTrue('&lt;test message 2&gt;' in rv)

    def test_timelines(self):
        """Make sure that timelines work"""
        self.register_and_login('foo', 'default')
        self.add_message('the message by foo')
        self.logout()
        self.register_and_login('bar', 'default')
        self.add_message('the message by bar')
        res = requests.get('http://localhost:5235/public').content.decode("utf-8")
        self.assertIn('the message by foo', res)
        self.assertIn('the message by bar', res)

        # bar's timeline should just show bar's message
        res = self.getContent(requests.get('http://localhost:5235/'))
        self.assertNotIn('the message by foo', res)
        self.assertIn('the message by bar', res)

        # now let's follow foo
        res = self.getContent(requests.get('http://localhost:5235/foo/follow'))
        #previous test , follow_redirects=True
        self.assertIn('You are now following &#34;foo&#34;', res) 

        # we should now see foo's message
        rv = self.getContent(requests.get(self.url + "/"))
        self.assertIn('the message by foo', rv.data)
        self.assertIn('the message by bar', rv.data)

        # but on the user's page we only want the user's message
        res = requests.get('http://localhost:5235/bar').content.decode("utf-8")
        self.assertNotIn('the message by foo', res)
        self.assertIn('the message by bar', res) 
        res = requests('http://localhost:5235/foo')
        self.assertIn('the message by foo', res) 
        self.assertNotIn('the message by bar', res) 

        # now unfollow and check if that worked
        res = requests.get('http://localhost:5235/foo/unfollow').content.decode("utf-8")
        self.assertIn('You are no longer following &#34;foo&#34;', res)
        res = requests.get('http://localhost:5235/').content.decode("utf-8")
        self.assertNotIn('the message by foo', res)
        self.assertIn('the message by bar', res)

if __name__ == '__main__':
    unittest.main()
