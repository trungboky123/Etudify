<script setup>
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
</script>
<template>
  <footer>
    <div class="footer-container">
      <div class="footer-column">
        <strong>{{ t('footer.contact') }}</strong>
        <div class="contact-item">
          <span>📧</span>
          <span>trunghiennguyen71@gmail.com</span>
        </div>
        <div class="contact-item">
          <span>📞</span>
          <span>+84 833210030</span>
        </div>
        <div class="contact-item">
          <span>📍</span>
          <span>18 Hoang Quoc Viet, Ha Noi</span>
        </div>
      </div>

      <div class="footer-column">
        <strong>{{ t('footer.aboutUs') }}</strong>
        <RouterLink class="social-item" to="#"> {{ t('footer.onlineLearning') }} </RouterLink>
        <RouterLink class="social-item" to="#"> {{ t('footer.certifiedCourses') }} </RouterLink>
        <RouterLink class="social-item" to="#"> {{ t('footer.trustedByStudents') }} </RouterLink>
      </div>

      <div class="footer-column">
        <strong>{{ t('footer.followUs') }}</strong>
        <RouterLink to="https://www.facebook.com/buihien.tiensi.58/" class="social-item" target="blank">
          Facebook
        </RouterLink>
        <RouterLink to="https://www.instagram.com/its.trhin/" class="social-item" target="blank">
          Instagram
        </RouterLink>
        <RouterLink to="https://www.tiktok.com/@ngtrh_hgbaov?lang=en" class="social-item" target="blank">
          Tiktok
        </RouterLink>
      </div>
    </div>

    <div class="footer-bottom">
      <p>{{ t('footer.copyright') }} © 2026 <b>Étudify</b>. {{ t('footer.rightsReserved') }}</p>
    </div>
  </footer>
</template>

<style scoped lang="scss">
footer {
  background: linear-gradient(135deg, #764ba2 0%, #667eea 100%);
  color: white;
  padding: 35px 20px 15px;
  margin-top: 60px;
}

.footer-container {
  max-width: 1000px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 30px;
  margin-bottom: 25px;
}

.footer-column {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.footer-column strong {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 3px;
  letter-spacing: 0.3px;
}

.contact-item {
  color: #e0e7ff;
  font-size: 13px;
  line-height: 1.6;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: color 0.3s ease;
}

.contact-item:hover {
  color: #ffffff;
}

.social-item {
  color: #e0e7ff;
  text-decoration: none;
  font-size: 13px;
  transition: all 0.3s ease;
  display: inline-block;
  position: relative;
  padding-left: 0;
}

.social-item:hover {
  color: #ffffff;
  transform: translateX(5px);
}

.social-item::before {
  content: '→';
  position: absolute;
  left: -20px;
  opacity: 0;
  transition: all 0.3s ease;
}

.social-item:hover::before {
  opacity: 1;
  left: -15px;
}

.footer-bottom {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  padding-top: 20px;
  text-align: center;
}

.footer-bottom p {
  margin: 0;
  color: #b8c5e0;
  font-size: 12px;
}

@media (max-width: 768px) {
  footer {
    padding: 30px 20px 15px;
  }

  .footer-container {
    gap: 25px;
  }

  .footer-column strong {
    font-size: 15px;
  }
}
</style>
