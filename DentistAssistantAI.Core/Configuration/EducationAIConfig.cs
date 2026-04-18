namespace DentistAssistantAI.Core.Configuration;

public static class EducationAIConfig
{
    public const string TeacherSystemPrompt = """
        You are DentEdu — an expert dental education assistant for faculty members.
        Generate high-quality academic content for dental students.
        Structure all output with clear markdown headings (## for sections).
        Respond in the same language as the request (Russian, English, or Uzbek).
        Be thorough, clinically accurate, and pedagogically sound.
        """;

    public const string StudentSystemPrompt = """
        You are DentTutor — a friendly, encouraging dental study assistant for students.
        Explain concepts clearly using simple language and helpful analogies.
        Avoid excessive jargon; when using a term, briefly define it.
        Structure answers with clear headings.
        Respond in the same language as the student's question (RU/EN/UZ).
        """;

    public const string ClinicalCaseSystemPrompt = """
        You are DentCase — a dental clinical case generator and evaluator.
        When generating cases: present realistic patient scenarios without revealing the diagnosis.
        When evaluating: give constructive, educational feedback with clear verdicts per criterion.
        Respond in the same language as the request.
        """;

    public static string LecturePromptTemplate(string topic, int year) => $"""
        Создай подробную лекцию на тему «{topic}» для {year} курса стоматологического факультета.

        Используй следующую структуру:
        ## 1. Введение и актуальность
        ## 2. Классификация
        ## 3. Состав и свойства
        ## 4. Показания и противопоказания
        ## 5. Клинический протокол (пошагово)
        ## 6. Преимущества и недостатки
        ## 7. Сравнение с альтернативами
        ## 8. Контрольные вопросы (5 вопросов)

        Уровень сложности: соответствует {year} курсу стоматологического факультета.
        """;

    public static string TestPromptTemplate(string topic, int year, int count) => $"""
        Составь тест из {count} вопросов по теме «{topic}» для студентов {year} курса стоматологического факультета.

        Формат каждого вопроса (строго соблюдай):
        **[N].** [Вопрос]
        А) [вариант]
        Б) [вариант]
        В) [вариант]
        Г) [вариант]
        ✅ **Правильный ответ:** [буква] — [краткое объяснение]

        Вопросы должны быть клинически ориентированными и соответствовать уровню {year} курса.
        """;

    public static string TeacherCasePromptTemplate(string topic, int year) => $"""
        Создай клинический кейс по теме «{topic}» для студентов {year} курса стоматологического факультета.

        Структура кейса (не указывай диагноз — студент должен поставить сам):
        ## ЖАЛОБЫ
        ## АНАМНЕЗ ЗАБОЛЕВАНИЯ
        ## АНАМНЕЗ ЖИЗНИ
        ## ДАННЫЕ ОСМОТРА
        ### Внешний осмотр
        ### Осмотр полости рта
        ### Зондирование, перкуссия, пальпация
        ## ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ИССЛЕДОВАНИЯ
        (рентгенологические данные и/или другие исследования)

        ---
        *(Поле для преподавателя)*
        ## ЭТАЛОННЫЙ ДИАГНОЗ
        ## ЭТАЛОННЫЙ ПЛАН ЛЕЧЕНИЯ
        """;

    public static string StudentCasePromptTemplate(string topic, int year) => $"""
        Создай клинический кейс по теме «{topic}» для студентов {year} курса стоматологического факультета.

        Структура кейса (НЕ указывай диагноз и план лечения — студент должен поставить сам):
        ## ЖАЛОБЫ
        ## АНАМНЕЗ ЗАБОЛЕВАНИЯ
        ## АНАМНЕЗ ЖИЗНИ
        ## ДАННЫЕ ОСМОТРА
        ### Внешний осмотр
        ### Осмотр полости рта
        ### Зондирование, перкуссия, пальпация
        ## ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ИССЛЕДОВАНИЯ

        Заверши кейс фразой:
        ---
        *На основании данных поставьте диагноз и составьте план лечения.*
        """;

    public static string StudentAskTemplate(string question, int year) => $"""
        Я студент {year} курса стоматологического факультета.

        {question}
        """;

    public static string CaseEvaluationTemplate(string caseText, string diagnosis, string treatment) => $"""
        КЛИНИЧЕСКИЙ КЕЙС:
        {caseText}

        ---
        ОТВЕТ СТУДЕНТА:
        **Диагноз:** {diagnosis}
        **План лечения:** {treatment}

        ---
        Оцени ответ студента по следующим критериям:

        ## 1. Диагноз
        Верность: ✅ Верно / ❌ Неверно / ⚠️ Частично верно
        Комментарий: [объяснение]

        ## 2. План лечения
        Верность: ✅ Верно / ❌ Неверно / ⚠️ Частично верно
        Комментарий: [объяснение]

        ## 3. Что упущено или что можно улучшить

        ## 4. Итоговая оценка
        **Отлично** / **Хорошо** / **Удовлетворительно** / **Неудовлетворительно**

        Будь конструктивным, образовательным и ободряющим в обратной связи.
        """;
}
