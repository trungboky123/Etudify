<script setup>
import GuessFooter from '@/components/GuessFooter.vue'
import api from '@/api/api'
import { onMounted, onUnmounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'

const courses = ref([])
const router = useRouter()
const { t } = useI18n()

const formatPrice = (price) => {
  return price.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' })
}

const fetchCourses = async () => {
  try {
    const res = await api.get('/courses/latest')
    const data = res.data
    courses.value = data
    console.log(courses.value)
  } catch (error) {
    console.error('Error fetching courses:', error)
  }
}
onMounted(() => {
  fetchCourses()
})

const banners = [
  {
    id: 1,
    courseName: 'Java Advanced',
    slug: 'java-advanced',
    courseId: 35,
    bgImage: 'https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=1200&h=500&fit=crop',
    bgColor: 'linear-gradient(135deg, rgba(102, 126, 234, 0.85) 0%, rgba(118, 75, 162, 0.85) 100%)',
  },
  {
    id: 2,
    courseName: 'Python Programming',
    slug: 'python-programming',
    courseId: 1,
    bgImage: 'https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?w=1200&h=500&fit=crop',
    bgColor: 'linear-gradient(135deg, rgba(249, 115, 22, 0.85) 0%, rgba(234, 88, 12, 0.85) 100%)',
  },
]

const handleBannerClick = (courseId) => {
  const course = banners.find((c) => c.courseId === courseId)
  router.push(`/public-course-details/${course.slug}/${courseId}`)
}

const handleCourseClick = (courseId) => {
  const course = courses.value.find((c) => c.id === courseId)
  router.push(`/public-course-details/${course.slug}/${courseId}`)
}

const activeSlide = ref(0)
let interval = null
onMounted(() => {
  interval = setInterval(() => {
    activeSlide.value = (activeSlide.value + 1) % banners.length
  }, 5000)
})
onUnmounted(() => {
  clearInterval(interval)
})

const handlePrevSlide = () => {
  activeSlide.value = (activeSlide.value - 1 + banners.length) % banners.length
}

const handleNextSlide = () => {
  activeSlide.value = (activeSlide.value + 1) % banners.length
}
</script>

<template>
  <div style="min-height: 100vh; background-color: #f8f9fa">
    <div class="banner-carousel">
      <div
        class="banner-slides-wrapper"
        :style="{ transform: `translateX(-${activeSlide * 100}%)` }"
      >
        <div
          v-for="banner in banners"
          :key="banner.id"
          class="banner-slide"
          :style="{ backgroundImage: `url(${banner.bgImage})` }"
        >
          <div class="banner-overlay" :style="{ backgroundColor: banner.bgColor }">
            <div class="banner-content">
              <h2>{{ banner.courseName }}</h2>
              <button class="btn btn-more" @click="() => handleBannerClick(banner.courseId)">
                {{ t('home.banner.more') }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="carousel-indicators">
        <button
          v-for="(banner, index) in banners"
          :key="index"
          :class="{ 'carousel-indicator': true, active: index === activeSlide }"
          @click="() => (activeSlide = index)"
        ></button>
      </div>

      <div class="carousel-arrow prev" @click="handlePrevSlide">
        <i class="bi bi-chevron-left"></i>
      </div>
      <div class="carousel-arrow next" @click="handleNextSlide">
        <i class="bi bi-chevron-right"></i>
      </div>
    </div>

    <div class="container" style="max-width: 1200px; padding-bottom: 100px">
      <h1 class="section-title">{{ t('home.highlightedCourses') }}</h1>
      <div class="row g-4">
        <div v-for="course in courses" :key="course.id" class="col-12 col-sm-6 col-lg-3">
          <div
            class="course-card"
            :style="{ backgroundImage: `url(${course.thumbnailUrl})` }"
            @click="() => handleCourseClick(course.id)"
          >
            <div class="course-info">
              <div class="course-name">{{ course.name }}</div>
              <div v-if="course.salePrice !== null" class="course-price">
                <span class="price-original">{{ formatPrice(course.listedPrice) }}</span>
                <span class="price-sale">{{ formatPrice(course.salePrice) }}</span>
              </div>

              <div v-else class="course-price">
                <span class="price-normal">{{ formatPrice(course.listedPrice) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <GuessFooter />
</template>

<style scoped lang="scss">
/* Banner Carousel */
.banner-carousel {
  position: relative;
  height: 500px;
  overflow: hidden;
  border-radius: 0 0 40px 40px;
  margin-bottom: 80px;
  margin-top: 70px;
}

.banner-slides-wrapper {
  position: relative;
  height: 100%;
  display: flex;
  transition: transform 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}

.banner-slide {
  position: relative;
  min-width: 100%;
  height: 100%;
  background-size: cover;
  background-position: center;
  flex-shrink: 0;
}

/* Blur effect thay vì overlay màu */
.banner-slide::before {
  content: '';
  position: absolute;
  inset: 0;
  backdrop-filter: blur(3px);
  -webkit-backdrop-filter: blur(3px);
  background: rgba(0, 0, 0, 0.2);
}

.banner-overlay {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: none !important;
}

.banner-content {
  text-align: center;
  color: white;
  z-index: 2;
  animation: fadeInUp 0.8s ease;
  text-shadow: 0 4px 12px rgba(0, 0, 0, 0.7);
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.banner-content h2 {
  font-size: 4rem;
  font-weight: 900;
  margin-bottom: 2rem;
  letter-spacing: -2px;
  line-height: 1.1;
  text-shadow:
    0 4px 20px rgba(0, 0, 0, 0.8),
    0 2px 8px rgba(0, 0, 0, 0.6);
}

.btn-more {
  background: white;
  color: #667eea;
  font-weight: 700;
  padding: 15px 50px;
  border-radius: 50px;
  text-transform: uppercase;
  letter-spacing: 1px;
  border: none;
  transition: all 0.3s;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
}

.btn-more:hover {
  transform: translateY(-3px);
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.4);
  background: #667eea;
  color: white;
}

/* Carousel Indicators */
.carousel-indicators {
  position: absolute;
  bottom: 30px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  justify-content: center;
  gap: 10px;
  z-index: 3;
  margin: 0;
}

.carousel-indicator {
  width: 40px;
  height: 4px;
  border-radius: 2px;
  background-color: rgba(255, 255, 255, 0.5);
  border: none;
  margin: 0;
  transition: all 0.3s;
  cursor: pointer;
}

.carousel-indicator.active {
  width: 60px;
  background-color: white;
}

/* Navigation Arrows */
.carousel-arrow {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 50px;
  height: 50px;
  background: rgba(255, 255, 255, 0.3);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border: 2px solid rgba(255, 255, 255, 0.5);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 3;
  transition: all 0.3s;
}

.carousel-arrow:hover {
  background: rgba(255, 255, 255, 0.9);
  border-color: white;
}

.carousel-arrow.prev {
  left: 30px;
}

.carousel-arrow.next {
  right: 30px;
}

.carousel-arrow i {
  font-size: 1.5rem;
  color: white;
}

.carousel-arrow:hover i {
  color: #667eea;
}

/* Courses Section */
.section-title {
  font-size: 2.5rem;
  font-weight: 800;
  color: #1e293b;
  margin-bottom: 3rem;
  text-align: center;
  position: relative;
  padding-bottom: 20px;
}

.section-title::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 80px;
  height: 4px;
  background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
  border-radius: 2px;
}

.course-card {
  position: relative;
  height: 320px;
  border-radius: 20px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  background-size: cover;
  background-position: center;
}

.course-card::before {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(
    to bottom,
    rgba(0, 0, 0, 0) 0%,
    rgba(0, 0, 0, 0.4) 50%,
    rgba(0, 0, 0, 0.8) 100%
  );
  transition: 0.4s ease;
}

.course-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 20px 40px rgba(102, 126, 234, 0.3);
}

.course-info {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  padding: 24px;
  z-index: 2;
  color: white;
}

.course-name {
  font-size: 1.125rem;
  font-weight: 700;
  margin-bottom: 12px;
  line-height: 1.3;
  text-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.course-price {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.price-original {
  text-decoration: line-through;
  opacity: 0.8;
  font-size: 0.9rem;
}

.price-sale {
  color: #fbbf24;
  font-weight: 800;
  font-size: 1.1rem;
}

.price-normal {
  font-weight: 700;
  font-size: 1.1rem;
}

/* Responsive */
@media (max-width: 768px) {
  .banner-content h2 {
    font-size: 2.5rem;
  }

  .banner-carousel {
    height: 400px;
    margin-bottom: 60px;
  }

  .carousel-arrow {
    width: 40px;
    height: 40px;
  }

  .carousel-arrow i {
    font-size: 1.2rem;
  }
}

@media (max-width: 576px) {
  .banner-content h2 {
    font-size: 2rem;
  }

  .banner-carousel {
    height: 350px;
  }

  .course-card {
    height: 280px;
  }

  .carousel-arrow.prev {
    left: 15px;
  }

  .carousel-arrow.next {
    right: 15px;
  }
}
</style>
