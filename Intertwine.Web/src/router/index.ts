import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import QuestionsView from '../views/QuestionsView.vue'
import WalletView from '../views/WalletView.vue'
import PaymentsView from '../views/PaymentsView.vue'
import UserAnswersView from '../views/UserAnswersView.vue'
import MyProfileView from '../views/MyProfileView.vue'
import ProfilePreviewView from '../views/ProfilePreviewView.vue'
import { useAuthStore } from '../stores/authStore'
import { pinia } from '../stores/pinia'

const isAuthenticated = () => useAuthStore(pinia).signedIn

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: () => (isAuthenticated() ? '/today' : '/login') },
    { path: '/login', name: 'login', component: HomeView },
    {
      path: '/today',
      name: 'today',
      component: QuestionsView,
      props: { library: false },
      meta: { requiresAuth: true },
    },
    {
      path: '/questions',
      name: 'questions',
      component: QuestionsView,
      props: { library: true },
      meta: { requiresAuth: true },
    },
    { path: '/wallet', name: 'wallet', component: WalletView, meta: { requiresAuth: true } },
    { path: '/payments', name: 'payments', component: PaymentsView, meta: { requiresAuth: true } },
    { path: '/answers', name: 'answers', component: UserAnswersView, meta: { requiresAuth: true } },
    { path: '/profile', name: 'profile', component: MyProfileView, meta: { requiresAuth: true } },
    {
      path: '/profile/preview',
      name: 'profile-preview',
      component: ProfilePreviewView,
      meta: { requiresAuth: true, navigation: 'profile' },
    },
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

router.beforeEach((to) => {
  if (to.meta.requiresAuth && !isAuthenticated()) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }
  if (to.name === 'login' && isAuthenticated()) return { name: 'today' }
})
