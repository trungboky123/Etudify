import en from './en.json'
import vi from './vi.json'
import fr from './fr.json'
import { createI18n } from 'vue-i18n'

const messages = {
  en,
  vi,
  fr
}

const lang = localStorage.getItem('lang') || 'en'

const i18n = createI18n({
  legacy: false,
  locale: lang,
  fallbackLocale: 'en',
  messages
})

export default i18n;
