// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import App from './App'
import router from './router'
import store from './store'
import Vuetify from 'vuetify'
import Raven from 'raven-js';
import RavenVue from 'raven-js/plugins/vue';
import RNMsgChannel from 'react-native-webview-messaging';
import * as VueGoogleMaps from 'vue2-google-maps'
import 'vuetify/dist/vuetify.min.css'
import '@fortawesome/fontawesome-free/css/all.css'
import {locates} from "./common/util";
import moment from "moment";
import firebase from "./common/firebase"
moment.locale('vi');

Raven
  .config('https://044c78991b114aebbfad9a13b6d85a63@sentry.io/1256786')
  .addPlugin(RavenVue, Vue)
  .install();

Vue.use(Vuetify, {
  iconfont: 'fa'
});
Vue.use(VueGoogleMaps, {
  load: {
    key: 'AIzaSyAxQfGcJgUa5iZSfXirufK8Lbb8i5qDnwQ',
    libraries: 'places, directions',
  }
});
Vue.use(Vuetify, {
  lang: {
    locales: locates,
    current: 'vn'
  }
});

const localToken = store.getters['authenticate/getlocalToken'];
if (localToken) {
  store.commit('authenticate/setToken', {token: localToken})
}

let bridgeTriedCount = 0;

function onBridgeReady(cb) {
  if (window.postMessage.length !== 1) {
    bridgeTriedCount++;
    if (bridgeTriedCount < 6) {
      setTimeout(function () {
        onBridgeReady(cb)
      }, 500);
    }
  } else {
    cb();
  }
}

onBridgeReady(function () {
  RNMsgChannel.sendJSON({
    type: 'ready'
  });
});

RNMsgChannel.on('json', json => {
  Raven.captureBreadcrumb({
    category: 'ITS-Web-Client',
    message: 'onJson',
    data: {json}
  });

  const {
    type,
    payload
  } = json;

  switch (type) {
    case 'expToken':
      store.dispatch('user/updateMobileToken', {
        ...payload
      });
      return;
    case 'GroupInvitation':
    case 'LocationSuggestion':
      router.push({
        name: 'Notification'
      });
      return;
    default:
      Raven.captureException(new Error('Invalid message type'));
      break;
  }
});

firebase.auth().getRedirectResult().then(function(result) {
  if (result.credential) {
    // This gives you a Google Access Token. You can use it to access the Google API.
    var token = result.credential.accessToken;
    // ...
  }
  // The signed-in user info.
  var user = result.user;

  console.debug('getRedirectResult', user, token);
}).catch(function(error) {
  // Handle Errors here.
  var errorCode = error.code;
  var errorMessage = error.message;
  // The email of the user's account used.
  var email = error.email;
  // The firebase.auth.AuthCredential type that was used.
  var credential = error.credential;
  // ...
});


Vue.config.productionTip = false;

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  store,
  components: {App},
  template: '<App/>'
});


