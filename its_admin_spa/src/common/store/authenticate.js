import axiosInstance from "../util/axiosInstance";
import RNMsgChannel from 'react-native-webview-messaging';
import moment from "moment";
import Raven from "raven-js"
import firebase from "../firebase";

// const root = "https://itssolutiong9.azurewebsites.net/";
const root = "http://localhost:59728/";

const googleProvider = new firebase.auth.GoogleAuthProvider();
const facebookProvider = new firebase.auth.FacebookAuthProvider();

export default {
  namespaced: true,
  state: {
    facebookAppId: "266318357470729",
    token: undefined,
  },
  getters: {
    isLoggedIn(state) {
      return !!state.token;
    },
    getlocalToken() {
      const tokenStr = localStorage.getItem('token');
      if (tokenStr) {
        const token = JSON.parse(tokenStr);
        let expire = moment(token.expires);
        let now = moment();

        if (now.isBefore(expire)) {
          return token;
        }
      }
    },
  },
  mutations: {
    setToken(state, payload) {
      const {
        token
      } = payload;

      let formattedToken;
      if (token.issued && token.expires) {
        //Formatted
        formattedToken = token;
      } else {
        formattedToken = {
          access_token: token.access_token,
          token_type: token.token_type,
          expires_in: token.expires_in,
          role: token.role,
          issued: token['.issued'],
          expires: token['.expires'],
        }
      }
      state.token = formattedToken;
      localStorage.setItem('token', JSON.stringify(formattedToken));
      axiosInstance.defaults.headers.common['Authorization'] =
        `${state.token.token_type} ${state.token.access_token}`;
    },
    nullToken(state) {
      state.token = undefined;
      localStorage.removeItem('token');
      axiosInstance.defaults.headers.common['Authorization'] = undefined;
    },
    setFacebookAuthentication(state, payload) {
      state.facebookStatus = payload.status;
    },
    setFacebookInstance(state, payload) {
      state.facebookInstance = payload.instance;
    },
    setLoading(state, payload) {
      state.loading = _.assign(state.loading, payload.loading);
    }
  },
  actions: {
    fetchToken(context, payload) {
      const {
        email,
        password
      } = payload;

      return new Promise((resolve, reject) => {
        const xhttp = new XMLHttpRequest();
        xhttp.open('POST', `${root}/token`, true);
        xhttp.send(`grant_type=password&username=${email}&password=${password}`);
        xhttp.onreadystatechange = function () {
          if (this.readyState == 4) {
            if (this.status == 200) {
              const responseObj = JSON.parse(this.responseText);
              resolve(responseObj);
            } else {
              let message = 'Có lỗi xảy ra';
              if (this.status === 400) {
                message = 'Sai tên đăng nhập hoặc mật khẩu'
              }
              reject({
                message
              });
            }
          }
        };
      });
    },
    signinFacebook(context) {
      // post /RegisterExternal
      firebase.auth().signInWithRedirect(facebookProvider);
    },
    signinGoogle(context) {
      // post /RegisterExternal
      firebase.auth().signInWithRedirect(googleProvider);

    },
    setExternalLoginToken(context, payload) {
      const {
        data
      } = payload;
      const obj = JSON.parse(data);
      context.commit('setToken',{token: obj});
      context.dispatch('user/fetchCurrentInfo', {}, {root: true});
      if (!!context.rootState.user.mobileToken) {
        context.dispatch('user/updateMobileToken', {}, {root: true});
      }
      Raven.captureBreadcrumb({
        message: 'signin',
        category: 'methods',
        data: {
          obj
        }
      });
    },
    logout(context) {
      context.dispatch('resetUserData', {}, {root: true});
      firebase.auth().signOut()
    }
  }
}
