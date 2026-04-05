import axios from "axios";

const instance = axios.create({
  baseURL: "http://localhost:5062",
  withCredentials: true
});

instance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // ❌ nếu không có response (network error) thì bỏ
    if (!error.response) {
      return Promise.reject(error);
    }

    // ❌ nếu request là refresh thì không retry nữa
    if (originalRequest.url.includes("/auth/refresh")) {
      return Promise.reject(error);
    }

    // ✅ access token hết hạn → thử refresh
    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        await instance.post("/auth/refresh");

        // ✅ retry lại request cũ
        return instance(originalRequest);
      } catch (refreshError) {
        // ❌ refresh fail → logout
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default instance;
