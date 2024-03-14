
import axios from 'axios';

// יצירת Instance של Axios עם הגדרת URL בסיס
const instance = axios.create({
  baseURL: 'http://localhost:5148'
});

// הוספת Interceptor לתגובות לטיפול בשגיאות
instance.interceptors.response.use(
  (response) => response, // העברת תגובות מוצלחות
  (error) => {
    console.error('שגיאה בתגובה:', error);
    return Promise.reject(error); // דחייה של Promise לטיפול נוסף
  }
);

export default {
  getTasks: async () => {
    const result = await instance.get('/items');
    return result.data;
  },

  addTask: async (name) => {
    try {
      const response = await instance.post('/items', { name });
      return response.data;
    } catch (error) {
      console.error('שגיאה בהוספת משימה:', error);
      throw error;
    }
  },

  setCompleted: async (id, isComplete) => {
    try {
      const response = await instance.put(`/items/${id}`, { isComplete });
      return response.data;
    } catch (error) {
      console.error('שגיאה בעדכון משימה:', error);
      throw error;
    }
  },

  deleteTask: async (id) => {
    try {
      await instance.delete(`/items/${id}`);
      return {}; // או החזרת הודעת הצלחה
    } catch (error) {
      console.error('שגיאה במחיקת משימה:', error);
      throw error;
    }
  }
};
