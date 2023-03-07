import { USER_TYPE_USER, USER_TYPE_ADMIN } from "@/consts/enums";
import * as urls from "@/consts/urls";

import AuthentificationService from "@/services/authentification";

// simplifies route config
const anonymous = true;
const notConnected = true;

/*
meta: {
  fullscreen: false, // If true, hides main app navigation
  anonymous: false, // If true, does not require login
  usertype: null, // set to USER_TYPE_USER / USER_TYPE_ADMIN / USER_TYPE_ANONYME to restrict page by user type
  notConnected: false // If true, the use need to be not connected to use this page
}
*/

// NOTE: On pourrait potentiellement splitter la config de routes en sous-groupes logiques et les combiner ici avec [...someRoutes, ...someOtherRoutes]
export default [
  {
    name: urls.URL_ROOT,
    path: "/",
    redirect() {
      const isLoggedIn = AuthentificationService.getUserIsLogged();

      if (!isLoggedIn) {
        return { name: urls.URL_LANDING };
      }

      const userType = AuthentificationService.getUserType();

      switch (userType) {
        case USER_TYPE_USER:
        case USER_TYPE_ADMIN:
          return {
            name: urls.URL_LIST_AD
          };
      }
    },
    meta: {
      anonymous
    }
  },
  {
    name: urls.URL_LANDING,
    path: "/accueil",
    component: () => import("@/components/pages/anonyme/landing-page.vue"),
    meta: {
      anonymous,
      fullscreen: true
    }
  },
  {
    name: urls.URL_SHARING_EQUIPMENT,
    path: "/equipement-a-partager",
    component: () => import("@/components/pages/anonyme/sharing-equipment.vue"),
    meta: {
      anonymous,
      fullscreen: true
    }
  },
  {
    name: urls.URL_LOOKING_FOR_EQUIPMENT,
    path: "/recherche-d-equipement",
    component: () => import("@/components/pages/anonyme/looking-for-equipment.vue"),
    meta: {
      anonymous,
      fullscreen: true
    }
  },
  {
    name: urls.URL_ABOUT,
    path: "/a-propos",
    component: () => import("@/components/pages/anonyme/about.vue"),
    meta: {
      anonymous,
      fullscreen: true
    }
  },
  {
    name: urls.URL_CONTACT,
    path: "/nous-joindre",
    component: () => import("@/components/pages/anonyme/contact.vue"),
    meta: {
      anonymous,
      fullscreen: true
    }
  },
  {
    name: urls.URL_LOGIN,
    path: "/connexion",
    component: () => import("@/components/pages/anonyme/login.vue"),
    meta: {
      notConnected,
      anonymous
    }
  },
  {
    name: urls.URL_FORGOT_PASSWORD,
    path: "/mot-de-passe-oublie",
    component: () => import("@/components/pages/anonyme/forgot-password.vue"),
    meta: {
      notConnected,
      anonymous
    }
  },
  {
    name: urls.URL_RESET_PASSWORD,
    path: "/reinitialiser-mot-de-passe",
    component: () => import("@/components/pages/anonyme/reset-password.vue"),
    meta: {
      notConnected,
      anonymous
    }
  },
  {
    name: urls.URL_RESEND_CONFIRMATION_EMAIL,
    path: "/reenvoyer-courriel",
    component: () => import("@/components/pages/anonyme/resend-email.vue"),
    meta: {
      notConnected,
      anonymous
    }
  },
  {
    name: urls.URL_USER_SUBSCRIBE,
    path: "/inscription",
    component: () => import("@/components/pages/anonyme/subscribe-user.vue"),
    meta: {
      notConnected,
      anonymous
    }
  },
  {
    key: urls.URL_CONFIRM_USER,
    path: "/confirmez-courriel",
    component: () => import("@/components/pages/anonyme/confirm-user.vue"),
    meta: {
      notConnected,
      anonymous
    }
  },
  {
    key: urls.URL_CONFIRM_ADMIN,
    path: "/registration/admin",
    component: () => import("@/components/pages/anonyme/confirm-admin.vue"),
    meta: {
      notConnected,
      anonymous
    }
  },

  {
    name: urls.URL_USER_PROFILE_DETAIL,
    path: "/profil/:id",
    component: () => import("@/components/pages/user-generic/profile-detail.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_PROFILE_EDIT,
    path: "/modifier-profil",
    component: () => import("@/components/pages/user-generic/profile-edit.vue"),
    meta: {
      usertype: [USER_TYPE_USER, USER_TYPE_ADMIN]
    }
  },
  {
    name: urls.URL_ACCOUNT_SETTINGS,
    path: "/parametres",
    component: () => import("@/components/pages/user-generic/account-settings.vue"),
    meta: {
      usertype: [USER_TYPE_USER, USER_TYPE_ADMIN]
    }
  },
  {
    name: urls.URL_MANAGE_ADS,
    path: "/gerer-annonces",
    component: () => import("@/components/pages/ad/manage.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },

  {
    name: urls.URL_LIST_AD,
    path: "/annonces",
    component: () => import("@/components/pages/ad/list.vue"),
    meta: {
      anonymous
    }
  },
  {
    name: urls.URL_CREATE_AD,
    path: "/annonces/creer",
    component: () => import("@/components/pages/ad/create.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_AD_DETAIL,
    path: "/annonces/:id",
    component: () => import("@/components/pages/ad/detail.vue"),
    meta: {
      anonymous
    }
  },
  {
    name: urls.URL_AD_EDIT,
    path: "/annonces/modifier/:id",
    component: () => import("@/components/pages/ad/edit.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_AD_ALERT_LIST,
    path: "/alertes",
    component: () => import("@/components/pages/alert/list.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_AD_ALERT_ADD,
    path: "/alertes/creer",
    component: () => import("@/components/pages/alert/add.vue"),
    meta: {
      anonymous
    }
  },
  {
    name: urls.URL_AD_ALERT_CONFIRM,
    path: "/alertes/confirmer/:id",
    component: () => import("@/components/pages/alert/confirm.vue"),
    meta: {
      anonymous
    }
  },
  {
    name: urls.URL_AD_ALERT_EDIT,
    path: "/alertes/modifier/:id",
    component: () => import("@/components/pages/alert/edit.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_AD_ALERT_DELETE,
    path: "/alertes/supprimer/:id",
    component: () => import("@/components/pages/alert/delete.vue"),
    meta: {
      anonymous
    }
  },

  {
    name: urls.URL_LIST_CONVERSATION,
    path: "/conversations",
    component: () => import("@/components/pages/conversation/list.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_CONVERSATION_DETAIL,
    path: "/conversations/:id",
    component: () => import("@/components/pages/conversation/detail.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_CREATE_CONVERSATION,
    path: "/conversations/creer/:adId",
    component: () => import("@/components/pages/conversation/create.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },

  /* Disable for Pilote version */
  /*{
    name: urls.URL_ADD_PAYMENT,
    path: "/conversations/:id/paiement/ajouter",
    component: () => import("@/components/pages/payment/add.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },*/

  /* Disable for Pilote version */
  /*{
    name: urls.URL_CREATE_CONTRACT,
    path: "/conversations/:id/contrat/creer",
    component: () => import("@/components/pages/contract/create.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_CONTRACT_DETAIL,
    path: "/contrat/:id",
    component: () => import("@/components/pages/contract/detail.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_CONTRACT_EDIT,
    path: "/contrat/:id/modifier",
    component: () => import("@/components/pages/contract/edit.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_CONTRACT_RATING,
    path: "/contrat/:id/evaluer",
    component: () => import("@/components/pages/contract/rate.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },*/

  /* Disable for Pilote version */
  /*{
    name: urls.URL_STRIPE_REFRESH,
    path: "/stripe-connect-refresh",
    component: () => import("@/components/pages/payment/refresh.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_STRIPE_VALIDATE,
    path: "/stripe-connect-validate",
    component: () => import("@/components/pages/payment/validate.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_PAYMENT_SUCCESS,
    path: "/payment-success",
    component: () => import("@/components/pages/payment/success.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },
  {
    name: urls.URL_PAYMENT_CANCEL,
    path: "/payment-cancel",
    component: () => import("@/components/pages/payment/cancel.vue"),
    meta: {
      usertype: USER_TYPE_USER
    }
  },*/

  {
    name: urls.URL_LIST_USERS,
    path: "/gestion-utilisateurs",
    component: () => import("@/components/pages/user-admin/manage-users.vue"),
    meta: {
      usertype: USER_TYPE_ADMIN
    }
  },
  {
    name: urls.URL_CREATE_ADMIN,
    path: "/gestion-utilisateurs/creer-administrateur",
    component: () => import("@/components/pages/user-admin/subscribe-admin.vue"),
    meta: {
      usertype: USER_TYPE_ADMIN
    }
  },
  {
    name: urls.URL_CREATE_USER,
    path: "/gestion-utilisateurs/creer-utilisateur",
    component: () => import("@/components/pages/user-admin/subscribe-user.vue"),
    meta: {
      usertype: USER_TYPE_ADMIN
    }
  },
  {
    name: urls.URL_EDIT_PROFILE,
    path: "/gestion-utilisateurs/modifier-utilisateur/:id",
    component: () => import("@/components/pages/user-admin/profile-edit.vue"),
    meta: {
      usertype: USER_TYPE_ADMIN
    }
  },

  {
    name: urls.URL_404,
    path: "*",
    component: () => import("@/components/pages/content/error-404.vue"),
    meta: {
      anonymous
    }
  }
];
